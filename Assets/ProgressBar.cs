using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

    private Slider slider;
    
    public float FillSpeed = 0.25f;
    private float targetProgress = 0;

    private void Awake()
    {
        slider = gameObject.GetComponent<Slider>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (slider.value < targetProgress && slider.value < slider.maxValue)
            slider.value += FillSpeed * Time.deltaTime;
    }

    public void IncrementProgress(float newProgress)
    {
        targetProgress = slider.value + newProgress;
    }

    public void SetProgress(float setProgress)
    {
        slider.value = setProgress;
        targetProgress = setProgress;
    }
}
