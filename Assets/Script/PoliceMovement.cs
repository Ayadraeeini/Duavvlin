using UnityEngine;
using System.Collections;

public class PoliceMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float speed = 2f;
    public float leftBound;
    public float rightBound;

    [Header("Detection Settings (General)")]
    public Transform player;
    public EyeDetectionUI eyeUI;
    public float verticalTolerance = 1.5f;

    [Header("Directional Detection")]
    public float frontRange = 5f;
    public float backRange = 1.25f;
    public float backFillMultiplier = 0.5f;

    [Header("Detection Fill/Drain Speeds")]
    public float detectionFillSpeed = 1f;
    public float detectionDrainSpeed = 1f;

    [Header("View Cone (optional)")]
    [SerializeField] private Transform viewCone;

    [Header("Turn Behaviour")]
    public float turnPause = 0.5f;
    public float turnCooldown = 1.0f;

    [Header("UI Recovery")]
    public float hardResetDelay = 0.6f;

    // --- State ---
    private bool isDetectingPlayer = false;
    private bool isChasing = false;
    private float loseSightCooldown = 0f;
    private bool movingRight = true;
    private SpriteRenderer sr;

    private bool isTurning = false;
    private float turnCooldownTimer = 0f;

    private float detectionMeter = 0f;
    private float outOfRangeTimer = 0f;

    // --- NEW ---
    private Vector3 startPosition;
    private bool startFacingRight;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Save initial spawn position & facing
        startPosition = transform.position;
        startFacingRight = movingRight;

        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (eyeUI == null && player != null)
            eyeUI = player.GetComponentInChildren<EyeDetectionUI>();

        if (viewCone != null && viewCone.GetComponent<Collider2D>() == null)
            Debug.LogWarning("[PoliceMovement] viewCone has no 2D Collider (optional). Detection uses math ranges.");
    }

    void Update()
    {
        if (turnCooldownTimer > 0f) turnCooldownTimer -= Time.deltaTime;

        bool stealthed = PlayerIsStealthed();

        if (stealthed)
        {
            isChasing = false;
            isDetectingPlayer = false;
            loseSightCooldown = 0f;
            DrainDetection();
            eyeUI?.StopDetection(detectionDrainSpeed);
        }

        if (!isTurning)
        {
            if (!isChasing) Patrol();
            else if (!stealthed) ChasePlayer();
        }

        DetectPlayer_Directional();

        if (!isDetectingPlayer && isChasing)
        {
            loseSightCooldown += Time.deltaTime;
            if (loseSightCooldown >= 2f)
            {
                isChasing = false;
                loseSightCooldown = 0f;
            }
        }
        if (isDetectingPlayer) loseSightCooldown = 0f;

        if (!stealthed) CheckAndTurnTowardPlayerBehind_Directional();
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
                RequestTurn(false, "Reached right bound");
            }
        }
        else
        {
            pos.x -= step;
            if (pos.x <= leftBound)
            {
                pos.x = leftBound;
                RequestTurn(true, "Reached left bound");
            }
        }

        transform.position = pos;
        sr.flipX = !movingRight;
    }

    void ChasePlayer()
    {
        if (player == null) return;
        if (isTurning) return;

        float step = speed * Time.deltaTime;
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        transform.position += direction * step;

        bool wantFaceRight = direction.x > 0;
        if (wantFaceRight != movingRight)
            RequestTurn(wantFaceRight, "Chase flip");

        sr.flipX = !movingRight;
    }

    void RequestTurn(bool faceRight, string reason)
    {
        if (isTurning) return;
        if (movingRight == faceRight) return;
        if (turnCooldownTimer > 0f) return;

        StartCoroutine(TurnAfterDelay(faceRight));
        turnCooldownTimer = turnCooldown;
    }

    IEnumerator TurnAfterDelay(bool faceRight)
    {
        isTurning = true;
        float elapsed = 0f;
        while (elapsed < turnPause) { elapsed += Time.deltaTime; yield return null; }
        FlipDirection(faceRight);
        isTurning = false;
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
            var childSR = child.GetComponent<SpriteRenderer>();
            if (childSR != null) childSR.flipX = !faceRight;
        }

        movingRight = faceRight;
        sr.flipX = !movingRight;
    }

    void DetectPlayer_Directional()
    {
        if (player == null) return;

        bool stealthed = PlayerIsStealthed();

        float dx = player.position.x - transform.position.x;
        float dy = player.position.y - transform.position.y;

        bool playerIsRight = dx > 0f;
        bool playerInFront = (movingRight && playerIsRight) || (!movingRight && !playerIsRight);

        float usedRange = playerInFront ? frontRange : backRange;
        bool inVertical = Mathf.Abs(dy) <= verticalTolerance;
        bool inHorizontal = Mathf.Abs(dx) <= usedRange;
        bool inRange = !stealthed && inVertical && inHorizontal;

        if (inRange)
        {
            outOfRangeTimer = 0f;

            float fill = playerInFront ? detectionFillSpeed : detectionFillMultiplier();
            detectionMeter += fill * Time.deltaTime;
            detectionMeter = Mathf.Clamp01(detectionMeter);
            eyeUI?.StartDetection(fill);

            if (!isDetectingPlayer)
            {
                isDetectingPlayer = true;
            }

            bool behindButVeryClose = !playerInFront && Mathf.Abs(dx) <= backRange * 0.6f;
            if (!isChasing && (playerInFront || behindButVeryClose) && detectionMeter < 1f)
            {
                isChasing = true;
            }

            if (detectionMeter >= 1f)
            {
                detectionMeter = 0f;
                eyeUI?.ResetDetection();
                var pm = player.GetComponent<Movement>();
                if (pm != null) pm.GetCaught();
            }
        }
        else
        {
            if (isDetectingPlayer) isDetectingPlayer = false;

            DrainDetection();
            eyeUI?.StopDetection(detectionDrainSpeed);

            outOfRangeTimer += Time.deltaTime;
            if (outOfRangeTimer >= hardResetDelay)
            {
                detectionMeter = 0f;
                eyeUI?.ResetDetection();
                outOfRangeTimer = 0f;
            }

            if (stealthed) isChasing = false;
        }
    }

    void CheckAndTurnTowardPlayerBehind_Directional()
    {
        if (player == null || isTurning) return;

        float dx = player.position.x - transform.position.x;
        float dy = player.position.y - transform.position.y;

        bool playerIsRight = dx > 0f;
        bool playerInFront = (movingRight && playerIsRight) || (!movingRight && !playerIsRight);

        bool behindAndClose = !playerInFront && Mathf.Abs(dx) <= backRange && Mathf.Abs(dy) <= verticalTolerance;

        if (behindAndClose && turnCooldownTimer <= 0f)
        {
            bool faceRight = player.position.x > transform.position.x;
            RequestTurn(faceRight, "Player approached from behind (close)");
        }
    }

    float detectionFillMultiplier()
    {
        return detectionFillSpeed * Mathf.Max(0f, backFillMultiplier);
    }

    void DrainDetection()
    {
        detectionMeter -= detectionDrainSpeed * Time.deltaTime;
        if (detectionMeter < 0f) detectionMeter = 0f;
    }

    bool PlayerIsStealthed()
    {
        if (player == null) return false;
        var pm = player.GetComponent<Movement>();
        return pm != null && pm.IsStealthed();
    }

    // NEW — public method to reset cop to start position/facing
    public void ResetPosition()
    {
        transform.position = startPosition;
        movingRight = startFacingRight;
        sr.flipX = !movingRight;

        if (viewCone != null)
        {
            Vector3 coneScale = viewCone.localScale;
            coneScale.x = movingRight ? Mathf.Abs(coneScale.x) : -Mathf.Abs(coneScale.x);
            viewCone.localScale = coneScale;
        }

        isDetectingPlayer = false;
        isChasing = false;
        detectionMeter = 0f;
        outOfRangeTimer = 0f;
        loseSightCooldown = 0f;
        eyeUI?.ResetDetection();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // front
        Vector3 frontCenter = transform.position + new Vector3(movingRight ? frontRange * 0.5f : -frontRange * 0.5f, 0f, 0f);
        Gizmos.DrawWireCube(frontCenter, new Vector3(frontRange, verticalTolerance * 2f, 0.1f));

        Gizmos.color = Color.yellow; // back
        Vector3 backCenter = transform.position + new Vector3(movingRight ? -backRange * 0.5f : backRange * 0.5f, 0f, 0f);
        Gizmos.DrawWireCube(backCenter, new Vector3(backRange, verticalTolerance * 2f, 0.1f));
    }
}
