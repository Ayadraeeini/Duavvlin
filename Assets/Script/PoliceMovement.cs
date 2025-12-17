using UnityEngine;
using System.Collections;

public class PoliceMovement : MonoBehaviour
{
    [Header("Patrol")]
    public float speed = 2f;
    public float leftBound;
    public float rightBound;

    [Header("Chase/Lose Sight")]
    public float loseSightSeconds = 2f;

    [Header("Turning")]
    public float turnPause = 0.5f;
    public float turnCooldown = 1.0f;

    [Header("Rendering (Sorting)")]
    public string characterSortingLayer = "Characters";
    public int characterOrder = 0;

    [Header("View Cone (child)")]
    [SerializeField] private Transform viewCone;
    private PatrolVisionCone cone;

    // State
    private bool isChasing = false;
    private bool movingRight = true;
    private bool isTurning = false;
    private float turnCooldownTimer = 0f;
    private float loseSightTimer = 0f;

    private SpriteRenderer sr;

    // spawn cache
    private Vector3 startPosition;
    private bool startFacingRight;

    // target
    public Transform player;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplySortingToSelfAndChildren();

        startPosition = transform.position;
        startFacingRight = movingRight;

        if (player == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found) player = found.transform;
        }

        cone = viewCone ? viewCone.GetComponent<PatrolVisionCone>() : null;
        if (cone == null)
            Debug.LogWarning("[PoliceMovement] ViewCone missing PatrolVisionCone component.");

        if (viewCone != null && viewCone.GetComponent<Collider2D>() == null)
            Debug.LogWarning("[PoliceMovement] viewCone has no 2D Collider. Add PolygonCollider2D (IsTrigger).");

        AlignConeToFacing();
    }

    void Update()
    {
        if (turnCooldownTimer > 0f) turnCooldownTimer -= Time.deltaTime;

        bool stealthed = PlayerIsStealthed();
        bool seePlayer = (!stealthed && cone != null && cone.IsTargetInSight);

        if (seePlayer)
        {
            isChasing = true;
            loseSightTimer = 0f;
        }
        else if (isChasing)
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseSightSeconds)
            {
                isChasing = false;
                loseSightTimer = 0f;
            }
        }

        if (!isTurning)
        {
            if (!isChasing) Patrol();
            else ChasePlayer();
        }
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
                RequestTurn(false);
            }
        }
        else
        {
            pos.x -= step;
            if (pos.x <= leftBound)
            {
                pos.x = leftBound;
                RequestTurn(true);
            }
        }

        transform.position = pos;
        sr.flipX = !movingRight;
    }

    void ChasePlayer()
    {
        if (player == null || isTurning) return;

        float step = speed * Time.deltaTime;
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        transform.position += dir * step;

        bool wantRight = dir.x > 0;
        if (wantRight != movingRight) RequestTurn(wantRight);

        sr.flipX = !movingRight;
    }

    void RequestTurn(bool faceRight)
    {
        if (isTurning || movingRight == faceRight || turnCooldownTimer > 0f) return;

        StartCoroutine(TurnAfterDelay(faceRight));
        turnCooldownTimer = turnCooldown;
    }

    IEnumerator TurnAfterDelay(bool faceRight)
    {
        isTurning = true;
        float t = 0f;
        while (t < turnPause)
        {
            t += Time.deltaTime;
            yield return null;
        }
        FlipDirection(faceRight);
        isTurning = false;
    }

    void FlipDirection(bool faceRight)
    {
        movingRight = faceRight;
        sr.flipX = !movingRight;

        AlignConeToFacing();

        foreach (Transform child in transform)
        {
            if (child == viewCone) continue;
            var childSR = child.GetComponent<SpriteRenderer>();
            if (childSR != null)
                childSR.flipX = !faceRight;
        }
    }

    void AlignConeToFacing()
    {
        if (!viewCone) return;

        // Flip cone using scale, not rotation (avoids Y drift)
        Vector3 scale = viewCone.localScale;
        scale.x = Mathf.Abs(scale.x) * (movingRight ? 1 : -1);
        viewCone.localScale = scale;

        // Preserve the same Y/Z position, just mirror X
        Vector3 pos = viewCone.localPosition;
        viewCone.localPosition = new Vector3(Mathf.Abs(pos.x) * (movingRight ? 1 : -1), pos.y, pos.z);
    }

    bool PlayerIsStealthed()
    {
        if (player == null) return false;
        var pm = player.GetComponent<Movement>();
        return pm != null && pm.IsStealthed();
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        movingRight = startFacingRight;
        sr.flipX = !movingRight;

        AlignConeToFacing();

        isChasing = false;
        isTurning = false;
        turnCooldownTimer = 0f;
        loseSightTimer = 0f;

        cone?.HardResetUI();
        ApplySortingToSelfAndChildren();
    }

    void ApplySortingToSelfAndChildren()
    {
        if (sr != null)
        {
            sr.sortingLayerName = characterSortingLayer;
            sr.sortingOrder = characterOrder;
        }

        var childSprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in childSprites)
        {
            if (!r) continue;
            r.sortingLayerName = characterSortingLayer;
            r.sortingOrder = Mathf.Max(characterOrder, r.sortingOrder);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (movingRight ? Vector3.right : Vector3.left) * 1.5f);
    }
}
