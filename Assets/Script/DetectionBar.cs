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
    private float targetValue = 0f;

    void Start()
    {
        if (slider == null)
        {
            Debug.LogError("[DetectionBar] Slider is not assigned!");
        }

        slider.value = 0f;
        sliderCanvas.SetActive(false);
    }

    void Update()
    {
        Debug.Log($"[DetectionBar] Update | Current: {currentValue:F2} | Target: {targetValue:F2}");

        currentValue = Mathf.MoveTowards(currentValue, targetValue, Time.deltaTime / fillTime);
        currentValue = Mathf.Clamp01(currentValue);
        slider.value = currentValue;
        sliderCanvas.SetActive(currentValue > 0f);
    }

    public void AddDetection(float delta)
    {
        Debug.Log($"[DetectionBar] AddDetection({delta:F4})");
        targetValue += delta / fillTime;
        targetValue = Mathf.Clamp01(targetValue);
    }

    public void RemoveDetection(float delta)
    {
        Debug.Log($"[DetectionBar] RemoveDetection({delta:F4})");
        targetValue -= delta / fillTime;
        targetValue = Mathf.Clamp01(targetValue);
    }

    public bool IsFull()
    {
        return currentValue >= 1f;
    }

    public void ResetBar()
    {
        Debug.Log("[DetectionBar] ResetBar called.");
        currentValue = 0f;
        targetValue = 0f;
        slider.value = 0f;
        sliderCanvas.SetActive(false);
    }
}
