using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataLogger : MonoBehaviour
{

    public static DataLogger Instance { get; private set; }

    private StreamWriter streamWriter;
    private string dataPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Should allow to call the Logger Initialization on each new scene
    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += InitializeLogger;
    }



    // Start is called before the first frame update
    void Start()
    {
        // SceneManager.sceneUnloaded += DisposeLogger;
    }

    // Proabably, it is worthwhile to dispose the streamWriter and than recreate it on the new scene
    // private void DisposeLogger(Scene current)
    // {
    //     if (streamWriter != null)
    //     {
    //         streamWriter.Dispose();
    //         streamWriter = null;
    //     }
    // }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitializeLogger(Scene scene, LoadSceneMode mode)
    {
        if (Application.isEditor)
        {
            dataPath = Application.dataPath + "/LogData/" +
            // SceneManager.GetActiveScene().name;
            scene.name;
        }
        else
        {
            dataPath = Application.persistentDataPath + "/LogData/" +
            // SceneManager.GetActiveScene().name
            scene.name;
        }

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        var logFilePath = Path.Combine(dataPath, timestamp + ".txt");

        try
        {
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            
            streamWriter = new StreamWriter(logFilePath);
            streamWriter.AutoFlush = true; // The write in the file after each WriteLine instead on Dispose().
        }
        catch (Exception e)
        {
            Instance.LogString("the creation of the new file on scene transition failed");
            Debug.LogError("Failed to open stream writer: " + e.Message);
            // _debugText.text += "Failed to open stream writer\n";
            return; // Early exit if file writing fails
        }

        Instance.LogString($"The scene name on the InitializeLogger is {scene.name}");
    }


    public void LogObserversState()
    {
        var spAwarenessSystem = (MixedRealitySpatialAwarenessSystem)CoreServices.SpatialAwarenessSystem;
        var meshObservers = spAwarenessSystem.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();

        foreach (var observer in meshObservers)
        {
            if (observer is BaseSpatialObserver baseObs)
            {
                Debug.Log($"Observer {observer.GetType().Name}: IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
                streamWriter.WriteLine($"Observer {observer.GetType().Name}: IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
                observer.Suspend();
                streamWriter.WriteLine($"Observer {observer.GetType().Name} state after suspension : IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
            }
        }
    }

    public void LogMeshData(List<SpatialMeshManager.SerializedMesh> meshes)
    {
        // streamWriter.WriteLine("The call after the Recreate Meshes");
        foreach (var mesh in meshes)
        {
            streamWriter.Write($"The mesh id is {mesh.meshId}");
        }
    }

    public void LogString(string message)
    {
        streamWriter.WriteLine(message);
    }
}
