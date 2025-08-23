using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class Movement : MonoBehaviour
{
    [Header("Input (Input System optional)")]
    public InputReader input;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpVelocity = 10f;
    public float gravityWhenNormal = 3f;

    [Header("Jump Tweaks")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;

    [Header("Climbing (optional)")]
    public float climbSpeed = 3f;

    [Header("Ground Check (optional)")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("UI (optional)")]
    public GameObject bustedImage;
    public float bustedDuration = 2f;

    [Header("Respawn")]
    public bool respawnToNearestSprayBox = true;
    public string sprayBoxTag = "SprayBox";
    public float behindTolerance = 0.05f;

    [Header("Axis Lock")]
    public float lockedZ = 0f;
    public bool lockZAlways = true;

    [Header("Rendering (Sorting)")]
    public string characterSortingLayer = "Characters";
    public int characterOrder = 0;

    [Header("Sound")]
    public AudioClip footstepClip;

    Rigidbody2D rb;
    SpriteRenderer sr;
    PlayerTransform playerTransform;
    Animator animator;
    AudioSource audioSource;

    bool isFrozen = false;
    bool onGround = false;
    bool nearLadder = false;
    bool isClimbing = false;
    bool nearHideSpot = false;
    bool isHiding = false;

    float inputX, inputY;
    private float speedMultiplier = 1f;

    private bool inputDisabled = false;
    private bool autoWalking = false;
    private float autoWalkSpeed = 0f;

    void ClampZ()
    {
        if (!lockZAlways) return;
        var p = transform.position;
        if (!Mathf.Approximately(p.z, lockedZ))
            transform.position = new Vector3(p.x, p.y, lockedZ);
    }

    void ApplySortingToSelfAndChildren()
    {
        if (sr != null)
        {
            sr.sortingLayerName = characterSortingLayer;
            sr.sortingOrder = characterOrder;
        }

        var childSprites = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var r in childSprites)
        {
            if (r == null) continue;
            r.sortingLayerName = characterSortingLayer;
            r.sortingOrder = Mathf.Max(characterOrder, r.sortingOrder);
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        playerTransform = GetComponent<PlayerTransform>();
        ApplySortingToSelfAndChildren();
        rb.gravityScale = gravityWhenNormal;
        if (bustedImage) bustedImage.SetActive(false);
        Time.timeScale = 1f;
        isFrozen = false;
        speedMultiplier = 1f;
        ClampZ();
    }

    void Start()
    {
        try
        {
            var spawn = GameObject.FindWithTag("SpawnPoint");
            if (spawn != null)
            {
                transform.position = spawn.transform.position;
                ClampZ();
                var sp = spawn.GetComponent<SpawnPoint>();
                if (sp != null) sr.flipX = !sp.faceRight;
            }
            else
            {
                ClampZ();
            }
        }
        catch (UnityException)
        {
            ClampZ();
        }

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isFrozen || (inputDisabled && !autoWalking)) return;

        if (animator != null)
            animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));

        Vector2 move = Vector2.zero;
        bool jumpPressed = false;
        bool hidePressed = false;
        bool restartPressed = false;

        if (input != null)
        {
            move = input.Move();
            jumpPressed = input.JumpPressed();
            hidePressed = input.HidePressed();
            restartPressed = input.PausePressed();
        }
        else
        {
            move.x = Input.GetAxisRaw("Horizontal");
            move.y = Input.GetAxisRaw("Vertical");
            jumpPressed = Input.GetKeyDown(KeyCode.Space);
            hidePressed = Input.GetKeyDown(KeyCode.H);
            restartPressed = Input.GetKeyDown(KeyCode.Return);
        }

        inputX = move.x;
        inputY = move.y;

        if (groundCheck)
            onGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            onGround = true;

        if (jumpPressed)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (onGround)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !isClimbing && !isHiding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (hidePressed && nearHideSpot)
        {
            isHiding = !isHiding;
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, isHiding ? 0f : 1f);
            if (isHiding) rb.velocity = Vector2.zero;
        }

        if (restartPressed)
        {
            ReloadSceneFresh();
        }

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

        // ✅ Footstep Sound Handling
        bool shouldPlayFootstep = Mathf.Abs(inputX) > 0.1f && onGround && !isHiding && !isClimbing;

        if (footstepClip != null)
        {
            if (shouldPlayFootstep)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = footstepClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying && audioSource.clip == footstepClip)
                {
                    audioSource.Stop();
                }
            }
        }

        ClampZ();
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        if (autoWalking)
        {
            rb.velocity = new Vector2(autoWalkSpeed, rb.velocity.y);
            return;
        }

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

        if (inputX > 0.1f) sr.flipX = false;
        else if (inputX < -0.1f) sr.flipX = true;
    }

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

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag("Ground")) onGround = true;
    }

    void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.CompareTag("Ground")) onGround = false;
    }

    public void GetCaught()
    {
        if (!isFrozen) StartCoroutine(FreezeShowBustedThenRespawn());
    }

    IEnumerator FreezeShowBustedThenRespawn()
    {
        isFrozen = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (bustedImage) bustedImage.SetActive(true);
        float wait = Mathf.Min(bustedDuration, 3f);
        yield return new WaitForSecondsRealtime(wait);

        Time.timeScale = 1f;

        if (respawnToNearestSprayBox)
        {
            Transform target = FindNearestSprayBoxBehind();
            if (target != null)
            {
                RespawnAt(target.position);
                yield break;
            }
        }

        ReloadSceneFresh();
    }

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
            float dx = px - go.transform.position.x;
            if (dx >= behindTolerance && dx < bestDelta)
            {
                bestDelta = dx;
                best = go.transform;
            }
        }

        return best;
    }

    void RespawnAt(Vector3 pos)
    {
        if (bustedImage) bustedImage.SetActive(false);
        transform.position = new Vector3(pos.x, pos.y, lockedZ);

        PoliceMovement[] cops = FindObjectsOfType<PoliceMovement>();
        foreach (var cop in cops) cop.ResetPosition();

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

        ApplySortingToSelfAndChildren();
        ClampZ();
    }

    public void ResetForNewScene(Transform spawn)
    {
        transform.position = new Vector3(spawn.position.x, spawn.position.y, lockedZ);

        PoliceMovement[] cops = FindObjectsOfType<PoliceMovement>();
        foreach (var cop in cops) cop.ResetPosition();

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

        ApplySortingToSelfAndChildren();
        ClampZ();
    }

    public bool IsStealthed()
    {
        return isHiding || (playerTransform != null && playerTransform.IsTransformed());
    }

    public void SetSpeedMultiplier(float m)
    {
        speedMultiplier = Mathf.Max(0f, m);
    }

    public void DisablePlayerInput()
    {
        inputDisabled = true;
    }

    public void AutoWalk(float speed)
    {
        autoWalking = true;
        autoWalkSpeed = speed;
    }
}
