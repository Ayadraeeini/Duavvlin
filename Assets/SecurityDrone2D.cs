using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SecurityDrone2D : MonoBehaviour
{
    [Header("Rotation (Ping-Pong)")]
    public float minAngle = -103f;
    public float maxAngle = 15f;
    public float rotationSpeedDegPerSec = 45f;

    [Header("Patrol (Horizontal)")]
    public float patrolDistance = 20f;
    public float patrolSpeed = 2f;
    public bool useRigidbodyForPatrol = false;

    [Header("Vision (no layers, no raycasts)")]
    [Range(5f, 179f)] public float fov = 90f;     // cone centered on local +X (right)
    public float viewDistance = 6f;
    public float verticalTolerance = 1.75f;

    [Header("Target")]
    public Transform player;

    [Header("Continuous Detection")]
    [Tooltip("Player must remain inside cone this long to bust.")]
    public float requiredContinuousSeconds = 4f;

    [Header("Eye UI")]
    public GameObject eyeCanvas;                 // image/sprite or UI object
    public CanvasGroup eyeCanvasGroup;           // optional, for fade
    public Transform eyeTargetOverride;          // e.g., Player/EyeAnchor
    public Vector3 eyeOffset = new Vector3(0f, 1.25f, 0f);
    public bool eyeFollowsPlayer = true;

    [Header("Cone Visual (optional)")]
    public Transform coneVisual;                 // scales X to match viewDistance

    [Header("Debug")]
    public bool showDebug = true;

    // --- internals ---
    Rigidbody2D _rb;
    Vector3 _startPos;
    float _currentAngle;
    int _dir = +1;

    Movement _playerMove;
    float _visibleTimer = 0f;   // continuous visible time
    float _detect01 = 0f;       // 0..1 (UI fade)
    bool _lastVisible = false;
    string _lastReason = "";
    bool _didStartupLog = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPos = transform.position;

        float z = transform.localEulerAngles.z;
        _currentAngle = (z > 180f) ? z - 360f : z;

        if (player != null) _playerMove = player.GetComponent<Movement>();

        if (eyeCanvas != null) eyeCanvas.SetActive(false);
        if (eyeCanvas != null && eyeCanvasGroup == null)
            eyeCanvasGroup = eyeCanvas.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (showDebug && !_didStartupLog)
        {
            Debug.Log($"[{name}] Drone ready. FOV={fov}°, dist={viewDistance}, vertTol={verticalTolerance}, bustAfter={requiredContinuousSeconds}s. Cone faces local +X.", this);
            _didStartupLog = true;
        }

        SweepHead();
        UpdateConeLength();

        bool visible = VisibleNow(out string reason, out float dist, out float ang);

        // --- debug logging (guaranteed on transitions / first tick) ---
        if (showDebug)
        {
            if (visible && !_lastVisible)
                Debug.Log($"[{name}] Visible -> dist={dist:F2}, angle={ang:F1}°, timer={_visibleTimer:F2}s", this);

            if (!visible && (_lastVisible || reason != _lastReason))
                Debug.Log($"[{name}] Not visible -> {reason} (dist={dist:F2}, angle={ang:F1}°)", this);
        }

        if (visible)
        {
            _visibleTimer += Time.deltaTime;
            _detect01 = Mathf.Clamp01(_visibleTimer / Mathf.Max(0.01f, requiredContinuousSeconds));

            if (_visibleTimer >= requiredContinuousSeconds)
            {
                if (showDebug) Debug.Log($"[{name}] BUST (continuous {requiredContinuousSeconds:F2}s inside cone).", this);
                TriggerBust();
                _visibleTimer = 0f;
                _detect01 = 0f;
                _lastVisible = false;
                _lastReason = "";
                if (eyeCanvas) eyeCanvas.SetActive(false);
                return;
            }
        }
        else
        {
            _visibleTimer = 0f;
            _detect01 = 0f;
        }

        _lastVisible = visible;
        _lastReason = reason;

        UpdateEyeCanvas(visible);
    }

    void FixedUpdate()
    {
        PatrolHorizontally();
    }

    // ---------------- Rotation ----------------
    void SweepHead()
    {
        _currentAngle += _dir * rotationSpeedDegPerSec * Time.deltaTime;

        if (_dir > 0 && _currentAngle >= maxAngle) { _currentAngle = maxAngle; _dir = -1; }
        else if (_dir < 0 && _currentAngle <= minAngle) { _currentAngle = minAngle; _dir = +1; }

        transform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
    }

    // ---------------- Patrol ----------------
    void PatrolHorizontally()
    {
        float d = Mathf.PingPong(Time.time * patrolSpeed, patrolDistance);
        float targetX = _startPos.x - d;
        Vector3 targetPos = new Vector3(targetX, _startPos.y, _startPos.z);

        if (useRigidbodyForPatrol && _rb != null) _rb.MovePosition(targetPos);
        else transform.position = targetPos;
    }

    // ---------------- Visibility (no layers) ----------------
    bool VisibleNow(out string reason, out float distOut, out float angleOut)
    {
        reason = "OK";
        distOut = 0f;
        angleOut = 999f;

        if (player == null) { reason = "no player assigned"; return false; }

        // Respect your Movement stealth (hide / transformed)
        if (_playerMove != null && _playerMove.IsStealthed())
        {
            reason = "player stealthed (hiding/transformed)";
            return false;
        }

        float vDelta = Mathf.Abs(player.position.y - transform.position.y);
        if (vDelta > verticalTolerance)
        {
            reason = $"vertical gate: |Δy|={vDelta:F2} > {verticalTolerance:F2}";
            return false;
        }

        Vector2 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude; distOut = dist;
        if (dist > viewDistance)
        {
            reason = $"too far: {dist:F2} > {viewDistance:F2}";
            return false;
        }

        Vector2 dir = toPlayer.normalized;
        float angle = Vector2.Angle(transform.right, dir); angleOut = angle;
        if (angle > fov * 0.5f)
        {
            reason = $"outside FOV: {angle:F1}° > {(fov * 0.5f):F1}°";
            return false;
        }

        // PASS: in cone by pure geometry (no LOS)
        return true;
    }

    // ---------------- Eye UI ----------------
    void UpdateEyeCanvas(bool visible)
    {
        if (eyeCanvas == null) return;

        bool shouldShow = visible || _detect01 > 0f;
        if (eyeCanvas.activeSelf != shouldShow) eyeCanvas.SetActive(shouldShow);
        if (!shouldShow) return;

        if (eyeCanvasGroup != null)
            eyeCanvasGroup.alpha = Mathf.Clamp01(_detect01);

        if (!eyeFollowsPlayer) return;

        Transform follow = eyeTargetOverride != null ? eyeTargetOverride : player;
        if (follow == null) return;

        Vector3 worldPos = follow.position + eyeOffset;

        if (eyeCanvas.TryGetComponent<RectTransform>(out var rt))
        {
            Canvas pc = rt.GetComponentInParent<Canvas>();
            if (pc != null && pc.renderMode != RenderMode.WorldSpace)
            {
                var cam = Camera.main;
                rt.position = cam ? cam.WorldToScreenPoint(worldPos)
                                  : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
                return;
            }
        }

        eyeCanvas.transform.position = worldPos; // world-space UI or sprite
    }

    void UpdateConeLength()
    {
        if (coneVisual == null) return;
        var ls = coneVisual.localScale;
        coneVisual.localScale = new Vector3(viewDistance, ls.y, ls.z);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 pos = transform.position;
        Vector3 fwd = transform.right;

        Vector3 left = Quaternion.Euler(0, 0, +fov * 0.5f) * fwd;
        Vector3 right = Quaternion.Euler(0, 0, -fov * 0.5f) * fwd;
        Gizmos.DrawLine(pos, pos + left * viewDistance);
        Gizmos.DrawLine(pos, pos + right * viewDistance);

        int steps = Mathf.Clamp(Mathf.RoundToInt(fov), 8, 90);
        Vector3 prev = pos + right * viewDistance;
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            float ang = Mathf.Lerp(-fov * 0.5f, +fov * 0.5f, t);
            Vector3 p = pos + (Quaternion.Euler(0, 0, ang) * fwd) * viewDistance;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
#endif

    void TriggerBust()
    {
        if (_playerMove != null) _playerMove.GetCaught();
        else if (showDebug) Debug.LogWarning($"[{name}] Player has no Movement.GetCaught(); cannot bust.", this);
    }
}
