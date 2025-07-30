using UnityEngine;
using UnityEngine.UI;

public class DetectionBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider slider;
    public GameObject sliderCanvas;

    [Header("Detection Settings")]
    public float fillTime = 4f;

    private float currentValue = 0f;
    private int detectionCount = 0;

    void Start()
    {
        slider.value = 0f;
        sliderCanvas.SetActive(false);
    }

    void Update()
    {
        bool isDetecting = detectionCount > 0;

        if (isDetecting)
        {
            currentValue += Time.deltaTime / fillTime;
            currentValue = Mathf.Clamp01(currentValue);
        }
        else
        {
            currentValue -= Time.deltaTime;
            currentValue = Mathf.Clamp01(currentValue);
        }

        slider.value = currentValue;
        sliderCanvas.SetActive(currentValue > 0f);
    }

    public void AddDetection()
    {
        detectionCount++;
    }

    public void RemoveDetection()
    {
        detectionCount = Mathf.Max(0, detectionCount - 1);
    }

    public bool IsFull()
    {
        return currentValue >= 1f;
    }

    public void ResetBar()
    {
        currentValue = 0f;
        slider.value = 0f;
        detectionCount = 0;
        sliderCanvas.SetActive(false);
    }
}
