using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class SessionMover : MonoBehaviour
{
    public PinchSlider sliderTransformX;
    public PinchSlider sliderTransformY;
    public PinchSlider sliderTransformZ;

    public Transform targetObject;

    private Vector3 initialPosition;

    void Start()
    {
        DataLogger.Instance.LogString($"The type of the Slider is {sliderTransformX.GetType()}");

        if (targetObject != null)
        {
            initialPosition = targetObject.position;
        }

        sliderTransformX.OnValueUpdated.AddListener(OnSliderValueChanged);
        sliderTransformY.OnValueUpdated.AddListener(OnSliderValueChanged);
        sliderTransformZ.OnValueUpdated.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(SliderEventData arg0)
    {
        UpdatePosition();
    }

    // private void OnSliderValueChanged(float value)
    // {
    //     UpdatePosition();
    // }

    void UpdatePosition()
    {
        if (targetObject == null)
        {
            DataLogger.Instance.LogString("The target Object on Update Postion is null");
            return;
        }

        float x = sliderTransformX.SliderValue;
        float y = sliderTransformY.SliderValue;
        float z = sliderTransformZ.SliderValue;

        targetObject.position = initialPosition + new Vector3(x, y, z);
    }
}
