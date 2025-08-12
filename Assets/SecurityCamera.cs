using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float minAngle = -143.92f;
    public float maxAngle = -48.6f;
    public float rotationSpeed = 30f;

    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public Transform player;
    public EyeDetectionUI eyeUI;
    public float detectionFillSpeed = 1f;
    public float detectionDrainSpeed = 1f;

    private bool rotatingForward = true;
    private float currentZ;

    private bool isDetectingPlayer = false;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (eyeUI == null && player != null)
        {
            eyeUI = player.GetComponentInChildren<EyeDetectionUI>();
        }

        currentZ = transform.eulerAngles.z;
    }

    void Update()
    {
        RotateCamera();
        DetectPlayer();
    }

    void RotateCamera()
    {
        float angleZ = transform.eulerAngles.z;
        // Convert Unity's 0–360 angle to -180 to 180 range
        if (angleZ > 180f) angleZ -= 360f;

        if (rotatingForward)
        {
            currentZ += rotationSpeed * Time.deltaTime;
            if (currentZ >= maxAngle)
            {
                currentZ = maxAngle;
                rotatingForward = false;
            }
        }
        else
        {
            currentZ -= rotationSpeed * Time.deltaTime;
            if (currentZ <= minAngle)
            {
                currentZ = minAngle;
                rotatingForward = true;
            }
        }

        transform.rotation = Quaternion.Euler(0f, 0f, currentZ);
    }

    void DetectPlayer()
    {
        if (player == null || eyeUI == null) return;

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement != null && playerMovement.IsStealthed())
        {
            if (isDetectingPlayer)
            {
                isDetectingPlayer = false;
                eyeUI.StopDetection(detectionDrainSpeed);
                Debug.Log($"[{gameObject.name}] Player is hiding or transformed. Stop detection.");
            }
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float angleToPlayer = Vector2.Angle(transform.right, toPlayer);
        float distanceToPlayer = toPlayer.magnitude;

        bool inRange = distanceToPlayer <= detectionRange && angleToPlayer < 45f;

        if (inRange)
        {
            if (!isDetectingPlayer)
            {
                isDetectingPlayer = true;
                eyeUI.StartDetection(detectionFillSpeed);
                Debug.Log($"[{gameObject.name}] Player ENTERED vision.");
            }

            if (eyeUI.IsFullyDetected())
            {
                eyeUI.ResetDetection();
                playerMovement.GetCaught();
                Debug.Log($"[{gameObject.name}] Player CAUGHT.");
            }
        }
        else
        {
            if (isDetectingPlayer)
            {
                isDetectingPlayer = false;
                eyeUI.StopDetection(detectionDrainSpeed);
                Debug.Log($"[{gameObject.name}] Player EXITED vision.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
