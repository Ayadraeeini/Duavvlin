using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 10f;
    public float climbSpeed = 3f;

    [Header("UI")]
    public GameObject bustedImage;
    public float bustedDuration = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerTransform playerTransformScript;

    private bool isJumping = false;
    private bool isClimbing = false;
    private bool isFrozen = false;
    private bool isHiding = false;
    private bool nearLadder = false;
    private bool nearHideSpot = false;
    private bool onGround = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerTransformScript = GetComponent<PlayerTransform>();

        if (bustedImage != null)
            bustedImage.SetActive(false);
    }

    void Update()
    {
        if (isFrozen) return;

        HandleInput();

        float verticalInput = Input.GetAxisRaw("Vertical");

        if (nearLadder)
        {
            if (Mathf.Abs(verticalInput) > 0)
            {
                isClimbing = true;
                rb.gravityScale = 0f;
            }
            else if (isClimbing && onGround && Mathf.Abs(rb.velocity.y) < 0.01f)
            {
                isClimbing = false;
                rb.gravityScale = 1f;
            }
        }

        if (!nearLadder)
        {
            isClimbing = false;
            rb.gravityScale = 1f;
        }

        if (isClimbing)
        {
            HandleClimbing();
        }
    }

    private void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");

        if (!isClimbing)
        {
            rb.velocity = new Vector2(moveX * speed, rb.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !isClimbing)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
        }

        if (Input.GetKeyDown(KeyCode.H) && nearHideSpot)
        {
            ToggleHiding();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            // The actual transform is handled inside PlayerTransform.cs
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("TutorialLevel");
        }
    }

    private void HandleClimbing()
    {
        float moveY = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveX * speed, moveY * climbSpeed);
    }

    private void ToggleHiding()
    {
        isHiding = !isHiding;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, isHiding ? 0f : 1f);
        rb.velocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearLadder = true;
        }
        else if (other.CompareTag("HideSpot"))
        {
            nearHideSpot = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearLadder = false;
            isClimbing = false;
            rb.gravityScale = 1f;
        }
        else if (other.CompareTag("HideSpot"))
        {
            nearHideSpot = false;
            if (isHiding)
                ToggleHiding();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            onGround = true;
            isJumping = false;

            if (isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = 1f;
                rb.velocity = Vector2.zero;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

    public void GetCaught()
    {
        StartCoroutine(FreezeShowBustedThenRestart());
    }

    private IEnumerator FreezeShowBustedThenRestart()
    {
        isFrozen = true;
        rb.velocity = Vector2.zero;

        if (bustedImage != null)
        {
            bustedImage.SetActive(true);
        }

        yield return new WaitForSeconds(bustedDuration);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsHiding() => isHiding;
    public bool IsTransformed() => playerTransformScript != null && playerTransformScript.IsTransformed();
    public bool IsStealthed() => IsHiding() || IsTransformed();
}
