using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmartSideScrollerCamera2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          // Player
    public Rigidbody2D targetRb;      // Optional (better speed-based zoom)

    [Header("Follow")]
    public Vector2 offset = new Vector2(0f, 1f);   // Base offset from player
    public Vector2 deadZone = new Vector2(1.2f, 0.6f);
    [Range(0.02f, 0.5f)] public float followDamp = 0.12f;

    [Header("Zoom")]
    public float baseSize = 5f;
    public float minSize = 4f;
    public float maxSize = 9f;
    public float zoomLerpSpeed = 4f;

    [Tooltip("Zoom out when player moves faster than this (units/s).")]
    public float speedZoomThreshold = 6f;
    public float speedZoomOutAmount = 2f;

    [Tooltip("Zoom out when player Y >= this world height.")]
    public float highYThreshold = 8f;
    public float highYZoomOutAmount = 2.5f;

    [Header("Bounds (optional)")]
    public Collider2D boundsCollider;

    // internals
    private Camera cam;
    private Vector3 vel;      // SmoothDamp velocity
    private float initialZ;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        initialZ = transform.position.z;
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // ---- POSITION ----
        Vector3 cur = transform.position;
        Vector2 cam2 = new Vector2(cur.x, cur.y);
        Vector2 anchor = (Vector2)target.position + offset;
        Vector2 delta = anchor - cam2;

        // Only move when leaving dead zone
        Vector3 desired = cur;
        if (Mathf.Abs(delta.x) > deadZone.x)
            desired.x = anchor.x - Mathf.Sign(delta.x) * deadZone.x;
        if (Mathf.Abs(delta.y) > deadZone.y)
            desired.y = anchor.y - Mathf.Sign(delta.y) * deadZone.y;
        desired.z = initialZ;

        // Smooth follow
        Vector3 pos = Vector3.SmoothDamp(cur, desired, ref vel, followDamp);

        // Optional clamp to level bounds
        if (boundsCollider)
            pos = ClampToBounds(pos, cam);

        transform.position = pos;

        // ---- ZOOM ----
        float desiredSize = baseSize;

        // Speed-based zoom (uses RB if provided)
        float speed = targetRb ? targetRb.velocity.magnitude : 0f;
        if (speed > speedZoomThreshold)
        {
            float t = Mathf.InverseLerp(speedZoomThreshold, speedZoomThreshold * 2f, speed);
            desiredSize += Mathf.Lerp(0f, speedZoomOutAmount, t);
        }

        // Height-based zoom
        if (target.position.y >= highYThreshold)
        {
            float t = Mathf.InverseLerp(highYThreshold, highYThreshold + 5f, target.position.y);
            desiredSize += Mathf.Lerp(0f, highYZoomOutAmount, t);
        }

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            Mathf.Clamp(desiredSize, minSize, maxSize),
            Time.deltaTime * zoomLerpSpeed
        );
    }

    Vector3 ClampToBounds(Vector3 desired, Camera c)
    {
        Bounds b = boundsCollider.bounds;

        float vert = c.orthographicSize;
        float horiz = c.orthographicSize * Mathf.Max(c.aspect, 0.0001f);

        float minX = b.min.x + horiz;
        float maxX = b.max.x - horiz;
        float minY = b.min.y + vert;
        float maxY = b.max.y - vert;

        // If bounds are smaller than camera extents, skip clamping to avoid popping
        if (minX > maxX || minY > maxY) return desired;

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);
        return desired;
    }
}