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
    public DetectionBar detectionBar;

    [Header("Detection Alert Sound")]
    public AudioSource alertAudio;
    private bool alertPlayed = false;
    private float alertTimer = 0f;

    private bool movingRight = true;
    private SpriteRenderer sr;

    private bool isCurrentlyDetecting = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Patrol();
        DetectPlayer();
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
                FlipAllChildren(false);
            }
        }
        else
        {
            pos.x -= step;
            if (pos.x <= leftBound)
            {
                pos.x = leftBound;
                movingRight = true;
                FlipAllChildren(true);
            }
        }

        transform.position = pos;
        sr.flipX = !movingRight;
    }

    void FlipAllChildren(bool faceRight)
    {
        foreach (Transform child in transform)
        {
            Vector3 scale = child.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            child.localScale = scale;
        }
    }

    void DetectPlayer()
    {
        if (player == null || detectionBar == null) return;

        Movement playerScript = player.GetComponent<Movement>();
        if (playerScript == null)
        {
            Debug.LogError("Movement script not found on player!");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange)
        {
            StopDetection();
            return;
        }

        float dirToPlayer = player.position.x - transform.position.x;
        bool playerInFront = (movingRight && dirToPlayer > 0) || (!movingRight && dirToPlayer < 0);

        if (!playerInFront)
        {
            StopDetection();
            return;
        }

        if (playerScript.IsHiding() || playerScript.IsTransformed())
        {
            StopDetection();
            return;
        }

        // Player is in view and not hiding
        if (!isCurrentlyDetecting)
        {
            detectionBar.AddDetection();
            isCurrentlyDetecting = true;
        }

        alertTimer += Time.deltaTime;

        if (!alertPlayed && alertTimer >= 0.1f)
        {
            alertAudio.Play();
            alertPlayed = true;
        }

        if (detectionBar.IsFull())
        {
            detectionBar.ResetBar();
            playerScript.GetCaught();
        }
    }

    void StopDetection()
    {
        if (isCurrentlyDetecting)
        {
            detectionBar.RemoveDetection();
            isCurrentlyDetecting = false;
        }

        alertPlayed = false;
        alertTimer = 0f;
    }
}
