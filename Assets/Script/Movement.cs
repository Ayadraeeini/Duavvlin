using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Movement : MonoBehaviour
{
    [Header("Input (Input System)")]
    public InputReader input; // ← drag your InputReader here (optional fallback to old input)

    [Header("Movement")]
    public float speed = 5f;
    public float jumpVelocity = 10f;       // vertical velocity
    public float gravityWhenNormal = 3f;

    [Header("Climbing (optional)")]
    public float climbSpeed = 3f;

    [Header("Ground Check (optional)")]
    public Transform groundCheck;          // empty at feet (optional)
    public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;          // set in Inspector

    [Header("UI (optional)")]
    public GameObject bustedImage;
    public float bustedDuration = 2f;      // will be capped to 3s

    [Header("Respawn")]
    public bool respawnToNearestSprayBox = true; // if false, always restart level
    public string sprayBoxTag = "SprayBox";      // tag for your spray/checkpoint objects
    public float behindTolerance = 0.05f;        // must be at least this much behind on X

    // --- Internals ---
    Rigidbody2D rb;
    SpriteRenderer sr;
    PlayerTransform playerTransform;       // your transform script

    bool isFrozen = false;
    bool onGround = false;
    bool nearLadder = false;
    bool isClimbing = false;
    bool nearHideSpot = false;
    bool isHiding = false;

    Animator animator;

    float inputX, inputY;

    // ✅ Speed multiplier hook (used by PlayerTransform)
    private float speedMultiplier = 1f;

    // --------------------------------------
    // Lifecycle
    // --------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerTransform = GetComponent<PlayerTransform>();

        // Base physics on load
        rb.gravityScale = gravityWhenNormal;

        // Ensure busted image is hidden on load
        if (bustedImage) bustedImage.SetActive(false);

        // Ensure the game is not globally paused on load
        Time.timeScale = 1f;
        isFrozen = false;

        // ensure default multiplier
        speedMultiplier = 1f;
    }

    void Start()
    {
        // Spawn at SpawnPoint if present (tag any Transform in your scene as "SpawnPoint")
        try
        {
            var spawn = GameObject.FindWithTag("SpawnPoint");
            if (spawn != null)
            {
                transform.position = spawn.transform.position;

                // Optional: if you have a SpawnPoint component with facing
                var sp = spawn.GetComponent<SpawnPoint>();
                if (sp != null) sr.flipX = !sp.faceRight;
            }
        }
        catch (UnityException)
        {
            // Tag not defined—ignore and start where placed
        }

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isFrozen) return;

        animator.SetFloat("xVelocity", Math.Abs(rb.velocity.x));

        // We now ALLOW movement while transformed.
        // Only hiding stops movement.

        // -------- Read input (Input System first, fallback to old Input) --------
        Vector2 move = Vector2.zero;
        bool jumpPressed = false;
        bool hidePressed = false;
        bool pausePressed = false;

        if (input != null)
        {
            move = input.Move();
            jumpPressed = input.JumpPressed();
            hidePressed = input.HidePressed();
            pausePressed = input.PausePressed();
        }
        else
        {
            // Fallback (keeps things working if InputReader not set)
            move.x = Input.GetAxisRaw("Horizontal");
            move.y = Input.GetAxisRaw("Vertical");
            jumpPressed = Input.GetKeyDown(KeyCode.Space);
            hidePressed = Input.GetKeyDown(KeyCode.H);
            pausePressed = Input.GetKeyDown(KeyCode.Return); // same as your quick restart
        }

        inputX = move.x;
        inputY = move.y;

        // ----------------------------------------------------------------------

        // Grounded check
        if (groundCheck)
            onGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Jump (allowed while transformed unless you want otherwise)
        if (jumpPressed && onGround && !isClimbing && !isHiding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }

        // Hide toggle
        if (hidePressed && nearHideSpot)
        {
            isHiding = !isHiding;
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, isHiding ? 0f : 1f);
            if (isHiding) rb.velocity = Vector2.zero;
        }

        // Quick restart (kept your Return behavior; also mapped to Pause action if you prefer)
        if (pausePressed)
        {
            ReloadSceneFresh();
        }

        // Ladder state
        if (nearLadder && Mathf.Abs(inputY) > 0.01f)
        {
            isClimbing = true;
            rb.gravityScale = 0f;
        }
        else if (!nearLadder)
        {
            isClimbing = false;
            rb.gravityScale = gravityWhenNormal;
        }
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        // Only hiding stops movement now.
        if (isHiding)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isClimbing)
        {
            rb.velocity = new Vector2(inputX * speed * speedMultiplier, inputY * climbSpeed);
        }
        else
        {
            rb.velocity = new Vector2(inputX * speed * speedMultiplier, rb.velocity.y);
        }

        // Face direction
        if (inputX > 0.1f) sr.flipX = false;
        else if (inputX < -0.1f) sr.flipX = true;
    }

    // --------------------------------------
    // Triggers for ladder/hide spots
    // --------------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder")) nearLadder = true;
        else if (other.CompareTag("HideSpot")) nearHideSpot = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearLadder = false;
            isClimbing = false;
            rb.gravityScale = gravityWhenNormal;
        }
        else if (other.CompareTag("HideSpot"))
        {
            nearHideSpot = false;
            if (isHiding)
            {
                isHiding = false;
                var c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
    }

    // --------------------------------------
    // Ground via collisions (backup)
    // --------------------------------------
    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag("Ground")) onGround = true;
    }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.CompareTag("Ground")) onGround = false;
    }

    // --------------------------------------
    // Busted flow → respawn to nearest SprayBox or restart
    // --------------------------------------
    public void GetCaught()
    {
        if (!isFrozen) StartCoroutine(FreezeShowBustedThenRespawn());
    }

    IEnumerator FreezeShowBustedThenRespawn()
    {
        isFrozen = true;

        // Stop player motion immediately
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Show busted overlay
        if (bustedImage) bustedImage.SetActive(true);

        // Wait in REAL time (ignores Time.timeScale), cap at 3s
        float wait = Mathf.Min(bustedDuration, 3f);
        yield return new WaitForSecondsRealtime(wait);

        // Ensure game unpaused before respawn / reload
        Time.timeScale = 1f;

        // Try respawn, else reload
        if (respawnToNearestSprayBox)
        {
            Transform target = FindNearestSprayBoxBehind();
            if (target != null)
            {
                RespawnAt(target.position);
                yield break;
            }
        }

        // Fallback: full reload
        ReloadSceneFresh();
    }

    // --------------------------------------
    // Helpers
    // --------------------------------------
    void ReloadSceneFresh()
    {
        if (bustedImage) bustedImage.SetActive(false);
        var sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }

    Transform FindNearestSprayBoxBehind()
    {
        GameObject[] sprays = GameObject.FindGameObjectsWithTag(sprayBoxTag);
        if (sprays == null || sprays.Length == 0) return null;

        float px = transform.position.x;
        Transform best = null;
        float bestDelta = Mathf.Infinity;

        foreach (var go in sprays)
        {
            float dx = px - go.transform.position.x; // positive if spray is behind
            if (dx >= behindTolerance)
            {
                if (dx < bestDelta)
                {
                    bestDelta = dx;
                    best = go.transform;
                }
            }
        }

        return best;
    }

    void RespawnAt(Vector3 pos)
    {
        if (bustedImage) bustedImage.SetActive(false);

        transform.position = pos;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = gravityWhenNormal;

        isFrozen = false;
        onGround = false;
        nearLadder = false;
        isClimbing = false;
        nearHideSpot = false;
        isHiding = false;

        speedMultiplier = 1f; // reset any transform speed change

        if (sr != null)
        {
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    public void ResetForNewScene(Transform spawn)
    {
        transform.position = spawn.position;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = gravityWhenNormal;

        isFrozen = false;
        onGround = false;
        nearLadder = false;
        isClimbing = false;
        nearHideSpot = false;
        isHiding = false;

        speedMultiplier = 1f;

        if (sr != null)
        {
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    // ✅ Needed for PoliceMovement & SecurityCamera
    public bool IsStealthed()
    {
        return isHiding || (playerTransform != null && playerTransform.IsTransformed());
    }

    // ✅ Hook for PlayerTransform to adjust speed in can-form
    public void SetSpeedMultiplier(float m)
    {
        speedMultiplier = Mathf.Max(0f, m);
    }
}
