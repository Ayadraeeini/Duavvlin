using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // CameraFollowPoint or Player
    public float smoothSpeed = 5f;
    public float elevatedYOffset = 3.4f;
    public float yThreshold = 0.05f; // Minimum movement to trigger offset change

    private float currentYOffset = 0f;
    private float previousY;

    void Start()
    {
        if (target != null)
            previousY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float deltaY = target.position.y - previousY;

        // Only increase offset if player clearly moves up
        float targetYOffset = deltaY > yThreshold ? elevatedYOffset : 0f;

        // Smoothly adjust Y offset (not instant!)
        currentYOffset = Mathf.Lerp(currentYOffset, targetYOffset, smoothSpeed * Time.deltaTime);

        // Follow target with smooth offset
        Vector3 desiredPosition = new Vector3(
            target.position.x,
            target.position.y + currentYOffset,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        previousY = target.position.y;
    }
}
