using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataLogger : MonoBehaviour
{
    // Temporal fields for debugging on Canvas
    public GameObject DebugText;
    private TMP_Text _debugText;
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

        if (DebugText != null)
        {
            _debugText = DebugText.GetComponent<TMP_Text>();
        }

        streamWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, "LogFile.txt"));
        _debugText.text += streamWriter;
    }

    // Should allow to call the Logger Initialization on each new scene
    void OnEnable()
    {
        Debug.Log("OnEnable for DataLogger is called");
        // Instance.LogString("OnEnable for DataLogger is called");
        SceneManager.sceneLoaded += InitializeLogger;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= InitializeLogger;
        // Close the stream writer when the object is disabled or destroyed.
        if (streamWriter != null)
        {
            streamWriter.Close();
            streamWriter.Dispose();
            streamWriter = null;
        }
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
        // if (streamWriter != null)
        // {
        //     streamWriter.Close();
        //     streamWriter.Dispose();
        //     streamWriter = null;
        // }

        if (Application.isEditor)
        {
            dataPath = Path.Combine(Application.dataPath, "LogData", scene.name);
            // dataPath = Application.dataPath + "/LogData/" +
            // // SceneManager.GetActiveScene().name;
            // scene.name;
        }
        else
        {
            dataPath = Path.Combine(Application.persistentDataPath, "LogData", scene.name);
            // dataPath = Application.persistentDataPath + "/LogData/" +
            // // SceneManager.GetActiveScene().name
            // scene.name;
        }

        Debug.Log($"The data path is {dataPath}");
        // Instance.LogString($"The data path is {dataPath}");
        _debugText.text += $"The data path is {dataPath}";

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        var logFilePath = Path.Combine(dataPath, timestamp + ".txt");

        Debug.Log($"The Log File Path is {logFilePath}");
        // Instance.LogString($"The Log File Path is {logFilePath}");
        _debugText.text += $"The Log File Path is {logFilePath}";

        try
        {
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
                // Instance.LogString($"Log after directory creation");
            }

            streamWriter = new StreamWriter(logFilePath);
            streamWriter.AutoFlush = true; // The write in the file after each WriteLine instead on Dispose().

            LogString($"Logger initialized for scene: {scene.name}");
            Debug.Log($"Logger initialized for scene: {scene.name}");
        }
        catch (Exception e)
        {
            Instance.LogString("the creation of the new file on scene transition failed");
            Debug.LogError("Failed to open stream writer: " + e.Message);
            _debugText.text += "Failed to open stream writer: " + e.Message;
            // _debugText.text += "Failed to open stream writer\n";
            // return; // Early exit if file writing fails
        }

        Instance.LogString($"The scene name on the InitializeLogger is {scene.name}");
        LogObserversState();
    }


    public void LogObserversState()
    {
        var spAwarenessSystem = (MixedRealitySpatialAwarenessSystem)CoreServices.SpatialAwarenessSystem;
        var meshObservers = spAwarenessSystem.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();

        foreach (var observer in meshObservers)
        {
            if (observer is BaseSpatialObserver baseObs && streamWriter != null)
            {
                Debug.Log($"Observer {observer.GetType().Name}: IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
                streamWriter.WriteLine($"Observer {observer.GetType().Name}: IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
                streamWriter.WriteLine($"Observer {observer.GetType().Name} state after suspension : IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
            }
        }
    }

    public void LogMeshData(List<SpatialMeshManager.SerializedMesh> meshes)
    {
        if (streamWriter != null)
        {
            // streamWriter.WriteLine("The call after the Recreate Meshes");
            foreach (var mesh in meshes)
            {
                streamWriter.Write($"The mesh id is {mesh.meshId}");
            }   
        }
    }

    public void LogString(string message)
    {
        if (streamWriter != null)
            streamWriter.WriteLine(message);
    }
}
