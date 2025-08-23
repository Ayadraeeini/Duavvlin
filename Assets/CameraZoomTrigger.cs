using UnityEngine;

public class CameraZoomTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera targetCamera;             // Assign Main Camera
    public float zoomedSize = 9.7f;         // Zoomed out size while inside
    public float defaultSize = 5f;          // Normal size when outside
    public float cameraYOffset = 3f;        // Extra height added when player is inside

    [Header("Follow Settings")]
    public Transform player;                // Drag the player Transform here
    public float followSpeed = 3f;          // Smoothness of Y movement

    [Header("Player Tag")]
    public string playerTag = "Player";

    private bool playerInside = false;
    private float defaultCamY;

    void Start()
    {
        if (targetCamera != null)
        {
            defaultCamY = targetCamera.transform.position.y;
        }
    }

    void Update()
    {
        if (playerInside && targetCamera != null && player != null)
        {
            // Smoothly follow the player’s Y position plus offset
            Vector3 camPos = targetCamera.transform.position;
            float targetY = player.position.y + cameraYOffset;
            camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * followSpeed);
            targetCamera.transform.position = camPos;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && targetCamera != null)
        {
            targetCamera.orthographicSize = zoomedSize;
            playerInside = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && targetCamera != null)
        {
            targetCamera.orthographicSize = defaultSize;
            playerInside = false;

            // Reset Y position
            Vector3 camPos = targetCamera.transform.position;
            camPos.y = defaultCamY;
            targetCamera.transform.position = camPos;
        }
    }
}
