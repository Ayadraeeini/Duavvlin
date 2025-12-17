using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class CopSecurityCone : MonoBehaviour
{
    [Header("Cone (FOV)")]
    [Range(5f, 179f)] public float fov = 90f;   // centered on local +X
    public float viewDistance = 6f;
    [Range(2, 80)] public int rayCount = 24;

    [Header("Line of Sight")]
    [Tooltip("Layers that BLOCK vision (Walls/Level). Player is auto-removed.")]
    public LayerMask obstacleMask = 0;

    [Header("Detection (same as SecurityCamera)")]
    public Transform player;                 // auto-find by tag 'Player' if null
    public EyeDetectionUI eyeUI;             // auto-find in player's children if null
    public float detectionFillSpeed = 1f;    // per second
    public float detectionDrainSpeed = 1f;   // per second
    public float hardResetDelay = 0.6f;      // match camera

    [Header("Debug")]
    public bool debugLogs = false;

    // internals
    private LineRenderer lr;
    private PolygonCollider2D poly;
    private bool playerInsideCone = false;   // set by trigger callbacks
    private float outOfRangeTimer = 0f;

    void Awake()
    {
        // Make sure the raycast never treats the Player as an obstacle
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0) obstacleMask &= ~(1 << playerLayer);
    }

    void Start()
    {
        if (player == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found) player = found.transform;
        }
        if (eyeUI == null && player != null)
            eyeUI = player.GetComponentInChildren<EyeDetectionUI>(true);

        if (player == null) Debug.LogWarning($"[{name}] No Player found (tag 'Player').");
        if (eyeUI == null) Debug.LogWarning($"[{name}] No EyeDetectionUI found under player.");

        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.positionCount = rayCount + 2; // origin + arc points
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;

        poly = GetComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.pathCount = 1;
    }

    void Update()
    {
        BuildConeGeometry();  // updates collider + line (orientation comes from transform)
        HandleDetectionTick();
    }

    // Builds the cone polygon (local) and line (world), identical to the camera
    void BuildConeGeometry()
    {
        int pointCount = rayCount + 2; // origin + (rayCount+1) arc points
        Vector2[] localPoly = new Vector2[pointCount];
        Vector3[] worldLine = new Vector3[pointCount];

        localPoly[0] = Vector2.zero;                 // polygon origin at local (0,0)
        Vector3 worldOrigin = transform.position;    // LR uses world positions
        worldLine[0] = worldOrigin;

        float half = fov * 0.5f;

        for (int i = 0; i <= rayCount; i++)
        {
            float t = (float)i / rayCount;
            float ang = Mathf.Lerp(-half, half, t);

            // local direction around +X
            Vector2 localDir = AngleToLocalDir(ang);

            // world direction for raycast clipping
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
            // drain while outside cone or stealthed
            eyeUI.StopDetection(detectionDrainSpeed);

            if (!playerInsideCone)
            {
                outOfRangeTimer += Time.deltaTime;
                if (outOfRangeTimer >= hardResetDelay)
                {
                    outOfRangeTimer = 0f;
                    eyeUI.ResetDetection();
                }
            }
            return;
        }

        // LOS recheck while inside trigger (same as camera)
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

            if (eyeUI.IsFullyDetected())
            {
                eyeUI.ResetDetection();
                pm?.GetCaught();
                if (debugLogs) Debug.Log($"[{name}] Player CAUGHT by cop.");
            }
        }
        else
        {
            eyeUI.StopDetection(detectionDrainSpeed);
        }
    }

    // --- Trigger like the SecurityCamera ---
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

    // local +X rotated by degrees → dir
    static Vector2 AngleToLocalDir(float degrees)
    {
        float r = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    public void HardResetDetection()
    {
        outOfRangeTimer = 0f;
        eyeUI?.ResetDetection();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * viewDistance);
    }
}
