using UnityEngine;
using UnityEngine.UI;

public class EyeDetectionUI : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite eyeClosed;
    public Sprite eyeHalfOpen;
    public Sprite eyeOpen;

    [Header("References")]
    public Image eyeImage;
    public GameObject eyeCanvas; // The root canvas object
    public Transform player;
    public Vector3 offset = new Vector3(0, 1.5f, 0); // World space offset above player

    [Header("Timers")]
    public float firstStageTime = 0.5f;
    public float secondStageTime = 2f;
    public float fullDetectionTime = 4f;
    public float timeBeforeReset = 3f;

    // Detection speed controls
    private float currentDetectionSpeed = 1f;
    private float currentDrainSpeed = 1f;

    private float detectionTimer = 0f;
    private int activeDetections = 0;
    private float lossTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (eyeCanvas != null)
            eyeCanvas.SetActive(true);
    }

    void Update()
    {
        // ✅ Follow player with screen position conversion
        if (player != null && eyeCanvas != null)
        {
            Vector3 worldPos = player.position + offset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            eyeCanvas.transform.position = screenPos;
        }

        // ✅ Handle detection logic
        if (activeDetections > 0)
        {
            detectionTimer += Time.deltaTime * currentDetectionSpeed;
            lossTimer = 0f;
        }
        else
        {
            lossTimer += Time.deltaTime;
            if (lossTimer >= timeBeforeReset)
            {
                Debug.Log("[EyeUI] Fully out of sight for 3s → Resetting.");
                ResetDetection();
            }
            else
            {
                detectionTimer = Mathf.MoveTowards(detectionTimer, 0f, Time.deltaTime * currentDrainSpeed);
            }
        }

        UpdateEyeSprite();
    }

    void UpdateEyeSprite()
    {
        if (detectionTimer < firstStageTime)
            eyeImage.sprite = eyeClosed;
        else if (detectionTimer < secondStageTime)
            eyeImage.sprite = eyeHalfOpen;
        else
            eyeImage.sprite = eyeOpen;
    }

    public void StartDetection(float detectionSpeed)
    {
        currentDetectionSpeed = detectionSpeed;
        activeDetections++;
        lossTimer = 0f;
        Debug.Log($"[EyeUI] StartDetection → activeDetections = {activeDetections}, speed = {detectionSpeed}");
    }

    public void StopDetection(float drainSpeed)
    {
        currentDrainSpeed = drainSpeed;
        activeDetections = Mathf.Max(0, activeDetections - 1);
        Debug.Log($"[EyeUI] StopDetection → activeDetections = {activeDetections}, drain = {drainSpeed}");
    }

    public bool IsFullyDetected()
    {
        return detectionTimer >= fullDetectionTime;
    }

    public void ResetDetection()
    {
        detectionTimer = 0f;
        activeDetections = 0;
        lossTimer = 0f;
        UpdateEyeSprite();
    }
}
