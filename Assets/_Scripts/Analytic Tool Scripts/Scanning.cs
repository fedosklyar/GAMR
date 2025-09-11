using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
// using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
// using Microsoft.MixedReality.Toolkit.SpatialObjectMeshObserver;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.XRSDK.WindowsMixedReality;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Scanning : MonoBehaviour, IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>
{
    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;
    private List<Mesh> spatialMeshes = new List<Mesh>();
    private int _updates;

    private int _adds;

    public int requiredUpdates = 50;

    public int requiredAdds = 30;

    public GameObject UpdateCount;
    private TMP_Text _updateCountText;
    public GameObject DebugText;
    private TMP_Text _debugText;

    public GameObject AddCount;

    private TMP_Text _addsCountText;

    string datapath;

    // StreamWriter streamWriter;


    void Awake()
    {
        _updates = 0;
        _adds = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        DataLogger.Instance.LogString("Start on Scanning is called");
        //I will try to display the debug text on Canvas
        _updateCountText = UpdateCount.GetComponent<TMP_Text>();
        _debugText = DebugText.GetComponent<TMP_Text>();
        _addsCountText = AddCount.GetComponent<TMP_Text>();
        // _debugText.text += "Update count text is: " + _updateCountText.text + "\n";

        DataLogger.Instance.LogString("GetComponent on text fields done");

        Debug.Log("Start on Scanning was called");
        var spAwarenessSystem = (MixedRealitySpatialAwarenessSystem)CoreServices.SpatialAwarenessSystem;

        // _updateCountText = UpdateCount.GetComponent<TMP_Text>();
        Debug.Log(_updateCountText);
        // spAwarenessSystem.RegisterHandler<IMixedRealitySpatialAwarenessObservationHandler<IMixedRealitySpatialAwarenessMeshObserver>>(this);
        // if (Application.isEditor)
        // {
        //     datapath = Path.Combine(Application.dataPath, "ReplayData", SceneManager.GetActiveScene().name);
        //     // datapath = Application.dataPath + "/ReplayData/" + SceneManager.GetActiveScene().name + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        // }
        // else
        // {
        //     datapath = Path.Combine(Application.persistentDataPath, "ReplayData", SceneManager.GetActiveScene().name);
        //     // datapath = Application.persistentDataPath + "/ReplayData/" + SceneManager.GetActiveScene().name;
        // }

        // string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        // var logFilePath = Path.Combine(datapath, timestamp + ".txt");
        // //Creation of the directory and streamWriter (actual text file)
        // try
        // {
        //     Directory.CreateDirectory(datapath);
        //     streamWriter = new StreamWriter(logFilePath);
        //     streamWriter.AutoFlush = true; // The write in the file after each WriteLine instead on Dispose().
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError("Failed to open stream writer: " + e.Message);
        //     _debugText.text += "Failed to open stream writer\n";
        //     return; // Early exit if file writing fails
        // }

        if (spAwarenessSystem != null)
        {
            DataLogger.Instance.LogString("Spatial Awareness is not null");
            // Will try to do the rescan in order to validate the issue witn the non-zero values of the counters
            // ClearAndRescan();

            // spAwarenessSystem.RegisterHandler<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>>(this);

            // meshObserver = spAwarenessSystem.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
            // Get ALL mesh observers
            // var meshObservers = spAwarenessSystem.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();

            // Find the active/enabled one
            meshObserver = spAwarenessSystem.GetActiveObserver();

            // foreach (var observer in meshObservers)
            // {
            //     if (observer is BaseSpatialObserver baseObserver && baseObserver.IsEnabled)
            //     {
            //         meshObserver = observer;
            //         Debug.Log($"Found active mesh observer: {observer.GetType().Name}");
            //         _debugText.text += $"Active observer: {observer}\n";
            //         break;
            //     }
            // }
            
            if (meshObserver != null)
            {
                DataLogger.Instance.LogString("Mesh Observer is not null");
                // Will try to omit the call of this method. Probably, it is not necessary actually
                // ClearAndRescan();
                spAwarenessSystem.RegisterHandler<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>>(this);
            }

            _debugText.text += $"Data provider is {meshObserver}\n";

            

            // this.spatialObjectMeshObserver = spAwarenesSystem.GetDataProvider<SpatialObjectMeshObserver>();
            // this.WindownsMeshObserver = spAwarenesSystem.GetDataProvider<WindowsMixedRealitySpatialMeshObserver>();
            if (Application.isEditor)
            {
                // 2 lines below are commented for the Universal Windows Platform, because the SpatialObjectMeshObserver is not applicable for that type of platform 
                // meshObserver = spAwarenessSystem.GetDataProvider<SpatialObjectMeshObserver>();
                // spAwarenessSystem.RegisterHandler<IMixedRealitySpatialAwarenessObservationHandler<SpatialObjectMeshObserver>>(this);
                // StartCoroutine(TestTextChange());
                // StartCoroutine(LoggingDictionaryState());
            }
            else
            {
                // meshObserver = spAwarenessSystem.GetDataProvider<WindowsMixedRealitySpatialMeshObserver>();
                // spAwarenessSystem.RegisterHandler<IMixedRealitySpatialAwarenessObservationHandler<WindowsMixedRealitySpatialMeshObserver>>(this);
            }

            //Will try to clear it in the start. 
            // Also it is important to disable it before clearence. Instead, it will be just suspended (disabled or stopped)
            // meshObserver.Disable();
            // meshObserver.ClearObservations();

            _debugText.text += $"Spatial Awareness system data is{spAwarenessSystem}\n";
            Debug.Log("Spatial Awareness system data is " + spAwarenessSystem);
            DataLogger.Instance.LogString($"Spatial Awareness system data is {spAwarenessSystem}");

            // And then resume
            // meshObserver.Enable();
            // meshObserver.Resume();
            // _debugText.text += "Mesh Observer data is " + meshObserver + "\n";
            DataLogger.Instance.LogString($"Mesh observer data is {meshObserver}");
            Debug.Log("Mesh Observer data is " + meshObserver);


            // StartCoroutine(ShowMeshData());
            // StartCoroutine(ScanningExperienceSimulation());
        }

    }

    //It would be more reliable to Dispose streamWriter, if the user will decide to close the application
    //during the scanning part
    // void OnDestroy()
    // {
    //     if (streamWriter != null)
    //     {
    //         streamWriter.Dispose();
    //         streamWriter = null;
    //     }
    // }


    public void ClearAndRescan()
    {
        // if (meshObserver is WindowsMixedRealitySpatialMeshObserver)
        if (meshObserver != null)
        {
            Debug.Log("Clearing old spatial observations and starting a new scan...");
            _debugText.text += "Clearing old spatial observations and starting a new scan...\n";
            
            meshObserver.Suspend();
            meshObserver.ClearObservations();
            
            // Reset counters after clearing in order to avoid the affect of the cached data
            _updates = 0;
            _adds = 0;

            // Update UI
            if (_updateCountText != null) _updateCountText.text = "Updates: 0";
            if (_addsCountText != null) _addsCountText.text = "Additions: 0";

            meshObserver.Resume();
        }
    }



    public void OnObservationAdded(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {
        Debug.Log("OnObservationAdded triggered.");

        // _debugText.text += "OnObservationAdded triggered." + "\n";

        ++_adds;

        if (_addsCountText != null)
            _addsCountText.text = "Additions: " + _adds.ToString();

        CheckFinishScan();

    }

    public void OnObservationUpdated(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {
        Debug.Log("OnObservationUpdated called");
        DataLogger.Instance.LogString("OnObservationUpdated called");
        // _debugText.text += "OnObservationUpdated called" + "\n";
        // This is where you would handle updates to the mesh, if needed.
        ++_updates;
        //Log the change with timestamp;

        // if (streamWriter != null)
        // {
            // streamWriter.WriteLine($"Updates: {_updates}, Dictionary Size: {meshObserver.Meshes.Count}, Time: {DateTime.UtcNow.TimeOfDay}");
            DataLogger.Instance.LogString($"Updates: {_updates}, Dictionary Size: {meshObserver.Meshes.Count}, Time: {DateTime.UtcNow.TimeOfDay}");
        // }

        _debugText.text += $"Dictionary Size: {meshObserver.Meshes.Count}\n";

        // Should comment for the next build
        if (_updateCountText != null)
            _updateCountText.text = "Updates: " + _updates.ToString();

        
        CheckFinishScan();
    }

    private void CheckFinishScan()
    {
        if (_updates > requiredUpdates && _adds > requiredAdds)
        {
            DataLogger.Instance.LogString("Within the if of CheckFinishScan");
            // temporal part just for logging
            var spAwarenessSystem = (MixedRealitySpatialAwarenessSystem)CoreServices.SpatialAwarenessSystem;
            var allObservers = spAwarenessSystem.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();

            foreach (var observer in allObservers)
            {
                if (observer is BaseSpatialObserver baseObs && baseObs.IsRunning)
                {
                    Debug.Log($"Suspending observer: {observer.GetType().Name}");
                    // streamWriter.WriteLine($"Suspending observer: {observer.GetType().Name}");
                    DataLogger.Instance.LogString($"Suspending observer: {observer.GetType().Name}");
                    // observer.Suspend();
                }
            }
            // temporal part just for logging

            // It seems that I faced with the null reference exception within it since I suspended
            // observers before querying the active one
            SpatialMeshManager.Instance.SerializeMeshes();

            // Suspend the observer to finish the scanning and data observation
            if (meshObserver != null)
            {
                DataLogger.Instance.LogString($"About to suspend observer: {meshObserver.GetType().Name}");

                DataLogger.Instance.LogString($"Observer state before suspend - IsEnabled: {((BaseSpatialObserver)meshObserver).IsEnabled}, IsRunning: {((BaseSpatialObserver)meshObserver).IsRunning}");

                meshObserver.Suspend();

                DataLogger.Instance.LogString($"Observer state after suspend - IsEnabled: {((BaseSpatialObserver)meshObserver).IsEnabled}, IsRunning: {((BaseSpatialObserver)meshObserver).IsRunning}");
            }

            // Unregister the handler and transition to the next scene
            CoreServices.SpatialAwarenessSystem.UnregisterHandler<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>>(this);
            SceneManager.LoadScene("LevelSelector");
        }
    }

    public void OnObservationRemoved(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {
        Debug.Log("OnObservationRemoved triggered.");
    }
}
