using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class EyeIndicator : MonoBehaviour
{
    [Header("Follow (World-Space)")]
    [Tooltip("Usually the Player transform. If left empty, tries to find a parent Player tag.")]
    public Transform followTarget;
    [Tooltip("Offset from the target (e.g., above the head).")]
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    [Header("UI References")]
    [Tooltip("CanvasGroup for fading the indicator in/out (optional but recommended).")]
    public CanvasGroup canvasGroup;
    [Tooltip("Optional radial/linear Image that fills with detection progress (0..1).")]
    public Image fillImage;

    [Header("Eye Stages (Optional)")]
    public GameObject stage1;
    public GameObject stage2;
    public GameObject stage3;

    [Header("Stage Thresholds")]
    [Range(0f, 1f)] public float stage1Threshold = 0.20f;
    [Range(0f, 1f)] public float stage2Threshold = 0.60f;
    [Range(0f, 1f)] public float stage3Threshold = 0.98f;

    [Header("Fading & Visibility")]
    public float fadeLerpSpeed = 10f;
    public bool autoHideWhenZero = true;

    // Runtime
    private float progress;            // 0..1 detection
    private float targetAlpha = 0f;    // canvasGroup target alpha
    private Camera mainCam;

    public float Progress => progress;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        mainCam = Camera.main;

        // Try to guess follow target if not set
        if (followTarget == null)
        {
            var playerTag = GameObject.FindGameObjectWithTag("Player");
            followTarget = playerTag != null ? playerTag.transform : transform.root;
        }

        // Start hidden
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        ApplyStageVisibility(0f);
        ApplyFill(0f);
    }

    void LateUpdate()
    {
        // Follow target (world-space)
        if (followTarget != null)
        {
            transform.position = followTarget.position + worldOffset;
        }

        // Smooth fade
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeLerpSpeed);
        }
    }

    // ---- API used by PoliceMovement ----

    public void StartDetect(float rate)
    {
        progress = Mathf.Clamp01(progress + rate * Time.deltaTime);
        ApplyVisuals(progress);
        targetAlpha = 1f; // ensure visible when detecting
    }

    public void StopDetect(float drainRate)
    {
        progress = Mathf.Clamp01(progress - drainRate * Time.deltaTime);
        ApplyVisuals(progress);

        if (autoHideWhenZero && progress <= 0.0001f)
            targetAlpha = 0f; // fade out when empty
    }

    public void ResetIndicator()
    {
        progress = 0f;
        ApplyVisuals(progress);
        if (autoHideWhenZero) targetAlpha = 0f;
    }

    public bool IsFullyDetected()
    {
        return progress >= 0.999f;
    }

    // ---- Internals ----

    private void ApplyVisuals(float p)
    {
        ApplyFill(p);
        ApplyStageVisibility(p);

        if (!autoHideWhenZero && canvasGroup != null && p <= 0f)
            targetAlpha = 1f;
    }

    private void ApplyFill(float p)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = p; // works for radial or horizontal if Image.Type=Filled
        }
    }

    private void ApplyStageVisibility(float p)
    {
        if (stage1 != null) stage1.SetActive(p >= stage1Threshold && p < stage2Threshold);
        if (stage2 != null) stage2.SetActive(p >= stage2Threshold && p < stage3Threshold);
        if (stage3 != null) stage3.SetActive(p >= stage3Threshold);
    }
}
