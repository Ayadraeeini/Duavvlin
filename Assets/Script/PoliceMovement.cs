using UnityEngine;

public class PoliceMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float speed = 2f;
    public float leftBound;
    public float rightBound;

    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public Transform player;
    public EyeDetectionUI eyeUI;

    [Header("View Cone")]
    [SerializeField] private Transform viewCone;

    private bool isDetectingPlayer = false;
    private bool isChasing = false;
    private float cooldownTimer = 0f;
    private bool movingRight = true;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (eyeUI == null && player != null)
        {
            eyeUI = player.GetComponentInChildren<EyeDetectionUI>();
        }
    }

    void Update()
    {
        if (!isChasing) Patrol();
        else ChasePlayer();

        DetectPlayer();

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
        {
            cooldownTimer = 0f;
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

    void DetectPlayer()
    {
        if (player == null || eyeUI == null) return;

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement != null && playerMovement.IsStealthed())
        {
            if (isDetectingPlayer)
            {
                isDetectingPlayer = false;
                eyeUI.StopDetection();
                Debug.Log($"[{gameObject.name}] Player is hiding or transformed. Stop detection.");
            }
            return;
        }

        float xDist = Mathf.Abs(transform.position.x - player.position.x);
        float yDist = Mathf.Abs(transform.position.y - player.position.y);
        bool inRange = xDist <= detectionRange && yDist <= 1.5f;

        if (inRange)
        {
            if (!isDetectingPlayer)
            {
                isDetectingPlayer = true;
                eyeUI.StartDetection();
                Debug.Log($"[{gameObject.name}] Player ENTERED vision.");
            }

            if (!isChasing && !eyeUI.IsFullyDetected())
            {
                isChasing = true;
            }

            if (eyeUI.IsFullyDetected())
            {
                eyeUI.ResetDetection();
                player.GetComponent<Movement>().GetCaught();
                Debug.Log($"[{gameObject.name}] Player CAUGHT.");
            }
        }
        else
        {
            if (isDetectingPlayer)
            {
                isDetectingPlayer = false;
                eyeUI.StopDetection();
                Debug.Log($"[{gameObject.name}] Player EXITED vision.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y, 0), new Vector3(detectionRange * 2, 3f, 0.1f));
    }
}
