using System;
using System.Reflection;
using UnityEngine;

public class PoliceMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float speed = 2f;
    public float leftBound;
    public float rightBound;

    [Header("Detection Settings")]
    [Tooltip("Max distance for LOS raycasts (cap)")]
    public float detectionRange = 8f;
    public Transform player;
    public EyeDetectionUI eyeUI;

    [Header("Detection Speed Settings")]
    public float detectionFillSpeed = 1f;
    public float detectionDrainSpeed = 1f;

    [Header("View Cone (must have a Collider2D)")]
    [SerializeField] private Transform viewCone;

    [Header("Line of Sight")]
    [Tooltip("Layers that block sight (Walls/Level). Do NOT include Player.")]
    public LayerMask obstructionMask;
    [Tooltip("Player layer (so Overlap/Raycast can 'see' the player).")]
    public LayerMask playerMask;

    private bool isDetectingPlayer = false;
    private bool isChasing = false;
    private float cooldownTimer = 0f;
    private bool movingRight = true;
    private SpriteRenderer sr;

    // Cone + LOS state
    private bool playerInCone = false;
    private bool hasLineOfSight = false;

    // Cached cone collider + filter + temp buffer
    private Collider2D coneCol;
    private ContactFilter2D playerFilter;
    private readonly Collider2D[] overlapResults = new Collider2D[8];

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (eyeUI == null && player != null)
            eyeUI = player.GetComponentInChildren<EyeDetectionUI>();

        if (viewCone != null)
        {
            coneCol = viewCone.GetComponent<Collider2D>();
            if (coneCol == null)
                Debug.LogError("[PoliceMovement] viewCone needs a Collider2D (Polygon or Box).");
            else
                coneCol.isTrigger = true; // ensure it never blocks
        }

        playerFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true
        };
        playerFilter.SetLayerMask(playerMask);
    }

    void Update()
    {
        if (!isChasing) Patrol();
        else ChasePlayer();

        // Update "in cone" using collider overlap (no extra script)
        playerInCone = IsPlayerInsideCone();

        // Update line of sight if inside cone
        hasLineOfSight = playerInCone && ComputeLineOfSight();

        DetectPlayer_Cone();

        // cooldown if lost
        if (!isDetectingPlayer && isChasing)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= 2f)
            {
                isChasing = false;
                cooldownTimer = 0f;
            }
        }

        if (isDetectingPlayer)
            cooldownTimer = 0f;
    }

    void Patrol()
    {
        float step = speed * Time.deltaTime;
        Vector3 pos = transform.position;

        if (movingRight)
        {
            pos.x += step;
            if (pos.x >= rightBound)
            {
                pos.x = rightBound;
                movingRight = false;
                FlipDirection(false);
            }
        }
        else
        {
            pos.x -= step;
            if (pos.x <= leftBound)
            {
                pos.x = leftBound;
                movingRight = true;
                FlipDirection(true);
            }
        }

        transform.position = pos;
        sr.flipX = !movingRight;
    }

    void ChasePlayer()
    {
        if (player == null) return;

        float step = speed * Time.deltaTime;
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        transform.position += direction * step;

        bool newFacingRight = direction.x > 0;

        if (newFacingRight != movingRight)
        {
            FlipDirection(newFacingRight);
            movingRight = newFacingRight;
        }

        sr.flipX = !movingRight;
    }

    void FlipDirection(bool faceRight)
    {
        if (viewCone != null)
        {
            Vector3 coneScale = viewCone.localScale;
            coneScale.x = faceRight ? Mathf.Abs(coneScale.x) : -Mathf.Abs(coneScale.x);
            viewCone.localScale = coneScale;
        }

        foreach (Transform child in transform)
        {
            if (child == viewCone) continue;

            SpriteRenderer childSR = child.GetComponent<SpriteRenderer>();
            if (childSR != null)
                childSR.flipX = !faceRight;
        }
    }

    bool IsPlayerInsideCone()
    {
        if (coneCol == null || player == null) return false;

        int count = coneCol.OverlapCollider(playerFilter, overlapResults);
        for (int i = 0; i < count; i++)
        {
            var col = overlapResults[i];
            if (col == null) continue;
            if (col.transform == player || col.transform.IsChildOf(player))
                return true;
        }
        return false;
    }

    bool ComputeLineOfSight()
    {
        if (player == null) return false;

        // Stealth check (optional). If not implemented, returns false and we keep going.
        if (PlayerIsStealthed())
            return false;

        Vector2 origin = viewCone != null ? (Vector2)viewCone.position : (Vector2)transform.position;
        Vector2 toPlayer = (Vector2)player.position - origin;

        float maxDist = Mathf.Min(toPlayer.magnitude, detectionRange <= 0f ? 9999f : detectionRange);

        int mask = obstructionMask | playerMask;
        var hits = Physics2D.RaycastAll(origin, toPlayer.normalized, maxDist, mask);

        bool los = false;
        foreach (var h in hits)
        {
            if (h.collider == null) continue;

            if (h.collider.transform == player)
            {
                los = true;   // first hit is player → visible
            }
            break;             // first hit decides either way
        }

        if (los)
        {
            bool facingRight = movingRight;
            bool playerIsRight = toPlayer.x > 0f;
            if (playerIsRight != facingRight)
                los = false; // optional: only in front
        }

        return los;
    }

    void DetectPlayer_Cone()
    {
        if (player == null || eyeUI == null) return;

        if (playerInCone && hasLineOfSight)
        {
            if (!isDetectingPlayer)
            {
                isDetectingPlayer = true;
                eyeUI.StartDetection(detectionFillSpeed);
                Debug.Log($"[{name}] Player VISIBLE in cone (LOS).");
            }

            if (!isChasing && !eyeUI.IsFullyDetected())
                isChasing = true;

            if (eyeUI.IsFullyDetected())
            {
                eyeUI.ResetDetection();
                var mv = player.GetComponent<Movement>();
                if (mv != null) mv.GetCaught();
                Debug.Log($"[{name}] Player CAUGHT.");
            }
        }
        else
        {
            if (isDetectingPlayer)
            {
                isDetectingPlayer = false;
                eyeUI.StopDetection(detectionDrainSpeed);
                Debug.Log($"[{name}] Player NOT visible (left cone or blocked).");
            }
        }
    }

    bool PlayerIsStealthed()
    {
        if (player == null) return false;

        var comps = player.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();

            var prop = t.GetProperty("IsStealthed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                try { if ((bool)prop.GetValue(c) == true) return true; } catch { }
            }

            var field = t.GetField("IsStealthed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                try { if ((bool)field.GetValue(c) == true) return true; } catch { }
            }

            var method = t.GetMethod("IsStealthed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method != null && method.ReturnType == typeof(bool))
            {
                try { if ((bool)method.Invoke(c, null) == true) return true; } catch { }
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (viewCone != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(viewCone.position, 0.07f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, transform.position.y, 0),
            new Vector3(detectionRange * 2f, 3f, 0.1f)
        );
    }
}
