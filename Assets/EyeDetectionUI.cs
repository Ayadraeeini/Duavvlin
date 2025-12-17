using UnityEngine;
using UnityEngine.UI;

public class EyeDetectionUI : MonoBehaviour
{
    [Header("Detection Meter")]
    [Tooltip("Assign a UI Image with 'Filled' mode (FillAmount 0–1)")]
    public Image detectionFillImage;

    [Tooltip("Optional max fill time in seconds (used for auto speed).")]
    public float maxFillTime = 3f;

    private float currentFillAmount = 0f;
    private float detectionSpeed = 0f;
    private float drainSpeed = 0f;
    private bool isDetecting = false;
    private bool isDraining = false;

    void Update()
    {
        if (isDetecting)
        {
            currentFillAmount += detectionSpeed * Time.deltaTime;
            currentFillAmount = Mathf.Clamp01(currentFillAmount);
        }
        else if (isDraining)
        {
            currentFillAmount -= drainSpeed * Time.deltaTime;
            currentFillAmount = Mathf.Clamp01(currentFillAmount);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (detectionFillImage != null)
        {
            detectionFillImage.fillAmount = currentFillAmount;
        }
    }

    public void StartDetection(float speed)
    {
        detectionSpeed = speed;
        isDetecting = true;
        isDraining = false;
    }

    public void StopDetection(float speed)
    {
        drainSpeed = speed;
        isDetecting = false;
        isDraining = true;
    }

    public void ResetDetection()
    {
        currentFillAmount = 0f;
        isDetecting = false;
        isDraining = false;
        UpdateUI();
    }

    public bool IsFullyDetected()
    {
        return currentFillAmount >= 1f;
    }

    // ✅ Needed by TriangleDetectionZone
    public float GetCurrentDetection()
    {
        return currentFillAmount;
    }
}
