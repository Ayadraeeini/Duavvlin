using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    public float jumpHeight;
    public float hideDetectionRange = 2f;
    public float climbSpeed = 3f;

    private float move;
    private bool isJumping = false;
    private bool isHiding = false;
    private bool isClimbing = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private GameObject currentHideSpot;
    private GameObject currentOutline;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckForNearbyHideSpot();

        // Toggle hiding
        if (currentHideSpot && Input.GetKeyDown(KeyCode.H))
        {
            isHiding = !isHiding;

            // Use alpha to hide/show
            float alpha = isHiding ? 0f : 1f;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

            rb.velocity = Vector2.zero;
        }

        if (isHiding)
            return;

        // Horizontal movement
        move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(move * speed, rb.velocity.y);

        // Climbing movement (vertical)
        if (isClimbing)
        {
            float vertical = Input.GetAxis("Vertical"); // W = +1, S = -1
            rb.velocity = new Vector2(rb.velocity.x, vertical * climbSpeed);
            rb.gravityScale = 0f; // Disable gravity when climbing
        }
        else
        {
            rb.gravityScale = 1f; // Normal gravity
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !isClimbing)
        {
            rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            isJumping = true;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.contacts[0].normal.y > 0.5f)
        {
            isJumping = false;
        }
    }

    // Enter ladder area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = true;
            rb.velocity = Vector2.zero;
        }
    }

    // Exit ladder area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = false;
        }
    }

    void CheckForNearbyHideSpot()
    {
        GameObject[] hideSpots = GameObject.FindGameObjectsWithTag("HideSpot");
        GameObject nearest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject spot in hideSpots)
        {
            float dist = Vector2.Distance(transform.position, spot.transform.position);
            if (dist < hideDetectionRange && dist < closestDistance)
            {
                closestDistance = dist;
                nearest = spot;
            }
        }

        if (nearest != currentHideSpot)
        {
            ClearCurrentOutline();

            if (nearest != null)
            {
                Transform outline = nearest.transform.Find("Outline");
                if (outline != null)
                {
                    outline.GetComponent<SpriteRenderer>().enabled = true;
                    currentOutline = outline.gameObject;
                }
            }

            currentHideSpot = nearest;
        }

        if (nearest == null)
        {
            ClearCurrentOutline();
            currentHideSpot = null;

            if (isHiding)
            {
                isHiding = false;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
            }
        }
    }

    void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.GetComponent<SpriteRenderer>().enabled = false;
            currentOutline = null;
        }
    }
}
