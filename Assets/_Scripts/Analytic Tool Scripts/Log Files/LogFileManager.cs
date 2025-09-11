using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;

public class LogFileManager : BaseObjectCollection
{
    public static LogFileManager logManager;
    string datapath;

    List<string> logfiles;
    List<GameObject> logFileButtons;

    public GameObject logfileMenu;
    public GameObject logfilebtn;
    public Transform logfiletarget;

    // StreamWriter streamWriter;

    private void Awake()
    {
        if (logManager == null)
        {
            logManager = this;
        }

        if (Application.isEditor)
        {
            datapath = Application.dataPath + "/ReplayData/" + SceneManager.GetActiveScene().name;
        }
        else
        {
            datapath = Application.persistentDataPath + "/ReplayData/" + SceneManager.GetActiveScene().name;
        }

        logfileMenu.SetActive(true);
        logfiles = new List<string>();
        logFileButtons = new List<GameObject>();

        // string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        // var logFilePath = Path.Combine(datapath, timestamp + ".txt");

        // try
        // {
        //     Directory.CreateDirectory(datapath);
        //     streamWriter = new StreamWriter(logFilePath);
        //     streamWriter.AutoFlush = true; // The write in the file after each WriteLine instead on Dispose().
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError("Failed to open stream writer: " + e.Message);
        //     // _debugText.text += "Failed to open stream writer\n";
        //     return; // Early exit if file writing fails
        // }
    }

    private void Start()
    {
        GetLogFiles();
        AddLogFileButtons();
        // LogObserverState();

        // Let the mesh recreation be here as well for now since the logging logic is present here
        // but, probably, not the best practice
        // SpatialMeshManager.Instance.RecreateMeshes();
        // LogMeshData();
    }

    // private void LogMeshData()
    // {
    //     streamWriter.WriteLine("The call after the Recreate Meshes");
    //     foreach (var mesh in SpatialMeshManager.persistentMeshes)
    //     {
    //         streamWriter.Write($"The mesh id is {mesh.meshId}");
    //     }
    // }

    protected override void LayoutChildren()
    {

    }

    public void GetLogFiles()
    {
        Debug.Log(datapath);
        string[] files = System.IO.Directory.GetFiles(datapath, "*.txt");

        foreach (string file in files)
        {
            logfiles.Add(file);
        }
    }

    public void AddLogFile(string path)
    {
        logfiles.Add(path);

        DeleteLogFileButtons();

        AddLogFileButtons();
    }

    public void ToggleLogFileMenu()
    {
        logfileMenu.SetActive(!logfileMenu.activeSelf);
        DisplayLogFileNames();
    }

    public void AddLogFileButtons()
    {
        foreach (string file in logfiles)
        {
            GameObject btn = Instantiate(logfilebtn, logfiletarget.position, transform.rotation);
            btn.transform.SetParent(logfiletarget);
            btn.GetComponent<ButtonConfigHelper>().MainLabelText = Path.GetFileName(file);
            logFileButtons.Add(btn);
        }

        StartCoroutine(InvokeUpdateCollection());
    }

    private IEnumerator InvokeUpdateCollection()
    {
        yield return null;
        logfiletarget.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    public string[] SelectedLogFiles()
    {
        List<string> selectedFiles;

        selectedFiles = new List<string>();

        foreach (GameObject btn in logFileButtons)
        {
            if (btn.GetComponent<CheckBox>().checkboxed)
            {
                Debug.Log(btn.GetComponent<ButtonConfigHelper>().MainLabelText);
                selectedFiles.Add(datapath + "/" + btn.GetComponent<ButtonConfigHelper>().MainLabelText);
            }

        }

        return selectedFiles.ToArray();
    }

    public void DeleteLogFileButtons()
    {
        foreach (Transform child in logfiletarget)
        {
            Destroy(child.gameObject);
        }
        logFileButtons.Clear();
    }

    public void DisplayLogFileNames()
    {
        foreach (string file in logfiles)
        {
            // Debug.Log(Path.GetFileName(file));
        }
    }

    // Will put the Spatial Observers suspension here for the beginnig
    //But obviously that it is not the best practice to have such logic in that class and in the described below method 
    // void LogObserverState()
    // {
    //     var spAwarenessSystem = (MixedRealitySpatialAwarenessSystem)CoreServices.SpatialAwarenessSystem;
    //     var meshObservers = spAwarenessSystem.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();

    //     foreach (var observer in meshObservers)
    //     {
    //         if (observer is BaseSpatialObserver baseObs)
    //         {
    //             Debug.Log($"Observer {observer.GetType().Name}: IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
    //             streamWriter.WriteLine($"Observer {observer.GetType().Name}: IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
    //             observer.Suspend();
    //             streamWriter.WriteLine($"Observer {observer.GetType().Name} state after suspension : IsEnabled={baseObs.IsEnabled}, IsRunning={baseObs.IsRunning}");
    //         }
    //     }
    // }


    
    // void OnDestroy()
    // {
    //     if (streamWriter != null)
    //     {
    //         streamWriter.Dispose();
    //         streamWriter = null;
    //     }
    // }
}
