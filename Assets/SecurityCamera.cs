using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SecurityCamera : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float minAngle = -143.92f;
    public float maxAngle = -48.6f;
    public float rotationSpeed = 30f;

    [Header("Cone (FOV)")]
    [Range(5f, 179f)] public float fov = 90f;
    public float viewDistance = 6f;
    [Range(2, 80)] public int rayCount = 24;

    [Header("Line of Sight")]
    public LayerMask obstacleMask = 0;

    [Header("Detection")]
    public Transform player;
    public EyeDetectionUI eyeUI;
    public float detectionFillSpeed = 1f;
    public float detectionDrainSpeed = 1f;
    public float hardResetDelay = 0.6f;

    [Header("Debug")]
    public bool debugLogs = false;

    // Internal
    private LineRenderer lr;
    private PolygonCollider2D poly;
    private float currentZ;
    private bool rotatingForward = true;

    private bool playerInsideCone = false;
    private float outOfRangeTimer = 0f;
    private bool alreadyCaught = false;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found) player = found.transform;
        }

        if (eyeUI == null && player != null)
            eyeUI = player.GetComponentInChildren<EyeDetectionUI>();

        if (player == null) Debug.LogWarning($"[{name}] No Player found (tag 'Player').");
        if (eyeUI == null) Debug.LogWarning($"[{name}] No EyeDetectionUI found under player.");

        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.positionCount = rayCount + 2;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;

        poly = GetComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.pathCount = 1;

        currentZ = transform.eulerAngles.z;
    }

    void Update()
    {
        RotateCamera();
        BuildConeGeometry();
        HandleDetectionTick();
    }

    void RotateCamera()
    {
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

    void BuildConeGeometry()
    {
        int pointCount = rayCount + 2;
        Vector2[] localPoly = new Vector2[pointCount];
        Vector3[] worldLine = new Vector3[pointCount];

        localPoly[0] = Vector2.zero;
        Vector3 worldOrigin = transform.position;
        worldLine[0] = worldOrigin;

        float half = fov * 0.5f;

        for (int i = 0; i <= rayCount; i++)
        {
            float t = (float)i / rayCount;
            float ang = Mathf.Lerp(-half, half, t);
            Vector2 localDir = AngleToLocalDir(ang);
            Vector2 worldDir = transform.TransformDirection(localDir);
            float dist = viewDistance;

            if (obstacleMask != 0)
            {
                RaycastHit2D hit = Physics2D.Raycast(worldOrigin, worldDir, viewDistance, obstacleMask);
                if (hit) dist = hit.distance;
            }

            Vector2 localPoint = localDir * dist;
            localPoly[i + 1] = localPoint;
            worldLine[i + 1] = transform.TransformPoint(localPoint);
        }

        poly.SetPath(0, localPoly);
        lr.SetPositions(worldLine);
    }

    void HandleDetectionTick()
    {
        if (player == null || eyeUI == null) return;

        Movement pm = player.GetComponent<Movement>();
        bool stealthed = (pm != null && pm.IsStealthed());

        if (!playerInsideCone || stealthed)
        {
            eyeUI.StopDetection(detectionDrainSpeed);

            if (!playerInsideCone)
            {
                outOfRangeTimer += Time.deltaTime;
                if (outOfRangeTimer >= hardResetDelay)
                {
                    outOfRangeTimer = 0f;
                    eyeUI.ResetDetection();
                    alreadyCaught = false;
                }
            }

            return;
        }

        // Optional LOS check
        bool hasLOS = true;
        if (obstacleMask != 0)
        {
            Vector2 origin = transform.position;
            Vector2 toPlayer = (Vector2)player.position - origin;
            var hit = Physics2D.Raycast(origin, toPlayer.normalized, toPlayer.magnitude, obstacleMask);
            hasLOS = !hit;
        }

        if (hasLOS)
        {
            outOfRangeTimer = 0f;
            eyeUI.StartDetection(detectionFillSpeed);

            if (eyeUI.IsFullyDetected() && !alreadyCaught)
            {
                alreadyCaught = true;
                eyeUI.ResetDetection();
                pm?.GetCaught();
                if (debugLogs) Debug.Log($"[{name}] Player CAUGHT by camera.");
            }
        }
        else
        {
            eyeUI.StopDetection(detectionDrainSpeed);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        playerInsideCone = true;
        if (debugLogs) Debug.Log($"[{name}] Player ENTER cone.");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        playerInsideCone = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        playerInsideCone = false;
        if (debugLogs) Debug.Log($"[{name}] Player EXIT cone.");
    }

    bool IsPlayer(Collider2D c)
    {
        if (player == null) return c.CompareTag("Player");
        return (c.attachedRigidbody != null)
            ? c.attachedRigidbody.transform == player
            : c.transform == player;
    }

    static Vector2 AngleToLocalDir(float degrees)
    {
        float r = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * viewDistance);
    }
}
