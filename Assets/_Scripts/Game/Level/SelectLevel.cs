using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class SelectLevel : MonoBehaviour, IMixedRealityTouchHandler
{
    [SerializeField]
    private int levelIndex;

    public void LoadLevel()
    {
        DataLogger.Instance.LogString($"The levelIndex value is {levelIndex}");
        SceneManager.LoadScene(levelIndex);
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData)
    {
        LoadLevel();
    }
    public void OnTouchCompleted(HandTrackingInputEventData eventData) {}
    public void OnTouchUpdated(HandTrackingInputEventData eventData) {}
}
