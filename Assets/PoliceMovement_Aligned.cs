using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class PoliceMovement_Aligned : MonoBehaviour
{
    [Header("Patrol")]
    public float speed = 2f;
    public float leftBound;
    public float rightBound;

    [Header("Turning")]
    public float turnPause = 0.3f;
    public float turnCooldown = 0.6f;

    [Header("Rendering (Sorting)")]
    public string characterSortingLayer = "Characters";
    public int characterOrder = 0;

    [Header("Cone Child")]
    [SerializeField] private Transform viewCone;     // child with CopInstantBuster + PolygonCollider2D + LineRenderer
    [SerializeField] private SpriteRenderer bodySpriteOverride; // optional; if null we use own SpriteRenderer

    // internals
    private bool movingRight = true;
    private bool isTurning = false;
    private float turnCooldownTimer = 0f;
    private SpriteRenderer sr;
    private Vector3 startPos;
    private bool startFacingRight;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (bodySpriteOverride == null) bodySpriteOverride = sr;
        ApplySortingToSelfAndChildren();
        startPos = transform.position;
        startFacingRight = movingRight;
    }

    void Start()
    {
        AlignConeToFacing(); // ensure cone points where we face at spawn
    }

    void Update()
    {
        if (turnCooldownTimer > 0f) turnCooldownTimer -= Time.deltaTime;
        if (!isTurning) Patrol();
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
        sr.flipX = !movingRight; // our art faces +X when flipX == false
    }

    void RequestTurn(bool faceRight)
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
        float t = 0f;
        while (t < turnPause) { t += Time.deltaTime; yield return null; }
        FlipDirection(faceRight);
        isTurning = false;
    }

    void FlipDirection(bool faceRight)
    {
        movingRight = faceRight;
        sr.flipX = !movingRight;
        AlignConeToFacing();
    }

    void AlignConeToFacing()
    {
        if (!viewCone) return;

        // Ensure cone uses ONLY rotation for direction (scale positive)
        var s = viewCone.localScale;
        s.x = Mathf.Abs(s.x); s.y = Mathf.Abs(s.y); s.z = Mathf.Abs(s.z);
        viewCone.localScale = s;

        // Our sprites look to the RIGHT when flipX == false.
        bool faceRight = !(bodySpriteOverride != null && bodySpriteOverride.flipX);
        // Point cone along local +X (transform.right)
        viewCone.localRotation = Quaternion.Euler(0f, 0f, faceRight ? 0f : 180f);
    }

    public void ResetPosition()
    {
        transform.position = startPos;
        movingRight = startFacingRight;
        sr.flipX = !movingRight;
        AlignConeToFacing();
        ApplySortingToSelfAndChildren();

        // Optional: reset the cone's UI if present
        var buster = viewCone ? viewCone.GetComponent<CopInstantBuster>() : null;
        if (buster) buster.HardResetDetection();
    }

    void ApplySortingToSelfAndChildren()
    {
        if (sr != null)
        {
            sr.sortingLayerName = characterSortingLayer;
            sr.sortingOrder = characterOrder;
        }
        var all = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in all)
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
