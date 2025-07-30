using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public Transform cameraTransform;        // Assign your camera
    public float targetY = -0.17f;           // Y position when in zone
    public float smoothSpeed = 3f;           // Smooth movement speed

    private Vector3 originalPosition;
    private bool isInZone = false;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        originalPosition = cameraTransform.position;
    }

    void Update()
    {
        Vector3 targetPos = cameraTransform.position;

        if (isInZone)
        {
            targetPos.y = targetY;
        }
        else
        {
            targetPos.y = originalPosition.y;
        }

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, Time.deltaTime * smoothSpeed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInZone = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInZone = false;
        }
    }
}
