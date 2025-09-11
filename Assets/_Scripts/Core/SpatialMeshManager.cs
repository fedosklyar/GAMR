using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows.Speech;

public class SpatialMeshManager : MonoBehaviour
{
    public Material visibleMaterial;

    public Material occlusionMaterial;

    [Header("Configuration")]
    public bool hasDedicatedScanningScene = true;
    public string scanningSceneName;

    public string menuSceneName;

    public static SpatialMeshManager Instance { get; private set; }

    [System.Serializable]
    public class SerializedMesh : UnityEngine.Object
    {
        // Geometry data (local to mesh)
        public Vector3[] vertices;
        public int[] triangles;
        public Vector3[] normals;

        // Critical: World transform data
        public Vector3 worldPosition;
        public Quaternion worldRotation;
        public Vector3 worldScale;

        // Mesh identifier
        public int meshId;

    }

    public static List<SerializedMesh> persistentMeshes = new List<SerializedMesh>();

    StreamWriter writer;

    // StreamReader reader;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void OnEnable()
    {
        Debug.Log("OnEnable called");

        SceneManager.sceneLoaded += MeshesRecreation;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= MeshesRecreation;
    }

    private void MeshesRecreation(Scene arg0, LoadSceneMode arg1)
    {
        if (hasDedicatedScanningScene && IsGameScene())
        {
            RecreateMeshes();
            DataLogger.Instance.LogMeshData(persistentMeshes);
            SuspendAllObservers();
            DataLogger.Instance.LogObserversState();
        }
    }

    // void Start()
    // {

    // }

    private void SuspendAllObservers()
    {
        var spAwarenessSystem = (MixedRealitySpatialAwarenessSystem)CoreServices.SpatialAwarenessSystem;
        var meshObservers = spAwarenessSystem.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();

        foreach (var observer in meshObservers)
        {
            if (observer is BaseSpatialObserver baseObs && baseObs.IsRunning)
            {
                observer.Suspend();
            }
        }
    }

    private bool IsGameScene()
    {
        return SceneManager.GetActiveScene().name != scanningSceneName && SceneManager.GetActiveScene().name != menuSceneName;
    }


    public void SerializeMeshes()
    {
        var meshObserver = CoreServices.SpatialAwarenessSystem.GetActiveObserver();

        foreach (var meshPair in meshObserver.Meshes)
        {
            var spatialMeshObject = meshPair.Value;
            var meshTransform = spatialMeshObject.GameObject.transform;

            var serializedMesh = new SerializedMesh
            {
                meshId = meshPair.Key,
                vertices = spatialMeshObject.Filter.mesh.vertices,
                triangles = spatialMeshObject.Filter.mesh.triangles,
                normals = spatialMeshObject.Filter.mesh.normals,
                worldPosition = meshTransform.position,
                worldRotation = meshTransform.rotation,
                worldScale = meshTransform.localScale
            };

            persistentMeshes.Add(serializedMesh);
        }

        DataLogger.Instance.LogString($"The serialized meshes list is {persistentMeshes.Count}");
    }

    public void RecreateMeshes()
    {
        // var meshObserver = (BaseSpatialMeshObserver)CoreServices.SpatialAwarenessSystem.GetActiveObserver();
        foreach (var serializedMesh in persistentMeshes)
        {
            // Will make the manager the parent object for the creted meshes

            GameObject meshObj = new GameObject($"SpatialMesh_{serializedMesh.meshId}");
            meshObj.transform.SetParent(this.transform);

            var mesh = new Mesh();
            mesh.vertices = serializedMesh.vertices;
            mesh.triangles = serializedMesh.triangles;
            mesh.normals = serializedMesh.normals;

            var meshFilter = meshObj.AddComponent<MeshFilter>();
            var meshRenderer = meshObj.AddComponent<MeshRenderer>();

            meshFilter.mesh = mesh;

            // Add physics
            var meshCollider = meshObj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false;

            meshRenderer.material = visibleMaterial;
            if (CoreServices.SpatialAwarenessSystem.GetActiveObserver() != null)
                meshObj.layer = CoreServices.SpatialAwarenessSystem.GetActiveObserver().MeshPhysicsLayer;
            else
                meshObj.layer = 31;

            meshObj.transform.SetPositionAndRotation(serializedMesh.worldPosition, serializedMesh.worldRotation);
            meshObj.transform.localScale = serializedMesh.worldScale;
        }
    }

    public void SaveMeshesInFile(string path, string dateTime)
    {
        // Probably, will need to rewrite the filename template and location 
        var filePath = Path.Combine(path, "SpatialData" + dateTime + ".txt");

        DataLogger.Instance.LogString($"The dataPath is {path}");
        DataLogger.Instance.LogString($"The filepath is {filePath}");
        try
        {
            writer = new StreamWriter(filePath);
            DataLogger.Instance.LogString("The file for spatial data storing created sucessfully");
        }
        catch (Exception e)
        {
            DataLogger.Instance.LogString("The file for spatial data storing was not created");
        }

        foreach (var mesh in persistentMeshes)
        {

            writer.WriteLine($"MESH_START:{mesh.meshId}");
            writer.WriteLine($"POSITION:{mesh.worldPosition.x},{mesh.worldPosition.y},{mesh.worldPosition.z}");
            writer.WriteLine($"ROTATION:{mesh.worldRotation.x},{mesh.worldRotation.y},{mesh.worldRotation.z},{mesh.worldRotation.w}");
            writer.WriteLine($"SCALE:{mesh.worldScale.x},{mesh.worldScale.y},{mesh.worldScale.z}");

            // Vertices
            writer.WriteLine($"VERTICES:{mesh.vertices.Length}");
            foreach (var vertex in mesh.vertices)
            {
                writer.WriteLine($"{vertex.x},{vertex.y},{vertex.z}");
            }

            // Triangles (indices into vertex array)
            writer.WriteLine($"TRIANGLES:{mesh.triangles.Length}");
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                writer.WriteLine($"{mesh.triangles[i]},{mesh.triangles[i + 1]},{mesh.triangles[i + 2]}");
            }

            // Normals
            writer.WriteLine($"NORMALS:{mesh.normals.Length}");
            foreach (var normal in mesh.normals)
            {
                writer.WriteLine($"{normal.x},{normal.y},{normal.z}");
            }

            writer.WriteLine("MESH_END");
        }

        writer.Close();
    }

    public void DeleteMeshes()
    {
        DataLogger.Instance.LogString("DeleteMeshes() called");
        foreach (Transform child in this.transform)
        {
            DataLogger.Instance.LogString($"childs name is {child.gameObject.name}");
            Destroy(child.gameObject);
        }

        DataLogger.Instance.LogString($"The length of the list before clearing is {persistentMeshes.Count}");
        persistentMeshes.Clear();
        DataLogger.Instance.LogString($"The length of the list after clearing is {persistentMeshes.Count}");
    }

    public void CreateMeshesFromFile(string path, string dateTime)
    {
        DataLogger.Instance.LogString($"dateTime in the CreateMeshesFromFile is {dateTime} and the path is {path}");
        var filePath = Path.Combine(path, "SpatialData" + dateTime + ".txt");

        DataLogger.Instance.LogString($"The Filepath is {filePath}");
        // try
        // {
        //     reader = new StreamReader(filePath);
        //     DataLogger.Instance.LogString("The file with spatial data located sucessfully");
        // }
        // catch (Exception e)
        // {
        //     DataLogger.Instance.LogString("The file with spatial data was not located");
        // }

        // string fileString = reader.ReadToEnd();
        // string[] lines = fileString.Split('\n');

        // foreach (string line in lines)
        // {

        // }

        // Will log the mesh data for the first mesh for debug purposes
        int counterForLogger = 0;

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                DataLogger.Instance.LogString("The file with spatial data located successfully");
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("MESH_START"))
                    {
                        SerializedMesh newMesh = new SerializedMesh();

                        // Read mesh ID
                        newMesh.meshId = int.Parse(line.Split(':')[1]);

                        // Will log the mesh data for the first mesh for debug purposes
                        if (counterForLogger < 3)
                            DataLogger.Instance.LogString($"The id is {newMesh.meshId}");

                        // Read position
                        string posLine = reader.ReadLine().Split(':')[1];
                        string[] posValues = posLine.Split(',');
                        newMesh.worldPosition = new Vector3(float.Parse(posValues[0]), float.Parse(posValues[1]), float.Parse(posValues[2]));
                        if (counterForLogger < 3)
                            DataLogger.Instance.LogString($"The pos values are {float.Parse(posValues[0])}, {float.Parse(posValues[1])}, {float.Parse(posValues[2])}");

                        // Read rotation
                        string rotLine = reader.ReadLine().Split(':')[1];
                        string[] rotValues = rotLine.Split(',');
                        newMesh.worldRotation = new Quaternion(float.Parse(rotValues[0]), float.Parse(rotValues[1]), float.Parse(rotValues[2]), float.Parse(rotValues[3]));

                        if (counterForLogger < 3)
                            DataLogger.Instance.LogString($"The rotation values are {float.Parse(rotValues[0])}, {float.Parse(rotValues[1])}, {float.Parse(rotValues[2])}, {float.Parse(rotValues[3])}");

                        // Read scale
                        string scaleLine = reader.ReadLine().Split(':')[1];
                        string[] scaleValues = scaleLine.Split(',');
                        newMesh.worldScale = new Vector3(float.Parse(scaleValues[0]), float.Parse(scaleValues[1]), float.Parse(scaleValues[2]));

                        if (counterForLogger < 3)
                            DataLogger.Instance.LogString($"The scale values are {float.Parse(scaleValues[0])}, {float.Parse(scaleValues[1])}, {float.Parse(scaleValues[2])}");

                        // Read vertices
                        string verticesLine = reader.ReadLine().Split(':')[1];
                        int numVertices = int.Parse(verticesLine);
                        newMesh.vertices = new Vector3[numVertices];
                        for (int i = 0; i < numVertices; i++)
                        {
                            string[] vertValues = reader.ReadLine().Split(',');
                            newMesh.vertices[i] = new Vector3(float.Parse(vertValues[0]), float.Parse(vertValues[1]), float.Parse(vertValues[2]));
                        }

                        // Read triangles
                        string trianglesLine = reader.ReadLine().Split(':')[1];
                        int numTriangles = int.Parse(trianglesLine);
                        newMesh.triangles = new int[numTriangles];
                        for (int i = 0; i < numTriangles; i += 3)
                        {
                            string[] triValues = reader.ReadLine().Split(',');
                            newMesh.triangles[i] = int.Parse(triValues[0]);
                            newMesh.triangles[i + 1] = int.Parse(triValues[1]);
                            newMesh.triangles[i + 2] = int.Parse(triValues[2]);
                        }

                        // Read normals
                        string normalsLine = reader.ReadLine().Split(':')[1];
                        int numNormals = int.Parse(normalsLine);
                        newMesh.normals = new Vector3[numNormals];
                        for (int i = 0; i < numNormals; i++)
                        {
                            string[] normalValues = reader.ReadLine().Split(',');
                            newMesh.normals[i] = new Vector3(float.Parse(normalValues[0]), float.Parse(normalValues[1]), float.Parse(normalValues[2]));
                        }

                        // Add to the list
                        persistentMeshes.Add(newMesh);

                        // To ensure the logging for n first meshes only
                        ++counterForLogger;
                    }
                }
            }
        }

        catch (Exception e)
        {
            DataLogger.Instance.LogString("The file with spatial data was not located or could not be read.");
            Debug.LogError($"Error reading spatial data file: {e.Message}");
        }

        // After list population from the file, the meshes will be created by already written method 
        RecreateMeshes();
    }
    
}
