using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MallGate2D_Auto : MonoBehaviour
{
    [Header("Door References")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Fixed Open Positions")]
    public float leftOpenX = 9.34f;
    public float rightOpenX = 9.78f;

    [Header("Motion Settings")]
    public float moveSpeed = 3f;

    [Header("Trigger Settings")]
    public string playerTag = "Player";

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private bool open = false;

    void Awake()
    {
        // Safety checks
        if (!leftDoor || !rightDoor)
        {
            Debug.LogError("[MallGate2D_Auto] Assign Left and Right door references in the inspector.");
            enabled = false;
            return;
        }

        // Store starting positions
        leftClosedPos = leftDoor.position;
        rightClosedPos = rightDoor.position;

        // Make sure collider is set as trigger
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        // Make sure this object has a Rigidbody2D so trigger works
        if (!GetComponent<Rigidbody2D>())
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }
    }

    void Update()
    {
        // Define target positions
        Vector3 leftTarget = open ? new Vector3(leftOpenX, leftClosedPos.y, leftClosedPos.z) : leftClosedPos;
        Vector3 rightTarget = open ? new Vector3(rightOpenX, rightClosedPos.y, rightClosedPos.z) : rightClosedPos;

        // Smoothly move doors
        leftDoor.position = Vector3.MoveTowards(leftDoor.position, leftTarget, moveSpeed * Time.deltaTime);
        rightDoor.position = Vector3.MoveTowards(rightDoor.position, rightTarget, moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[MallGate2D_Auto] Player entered trigger.");
            open = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[MallGate2D_Auto] Player exited trigger.");
            open = false;
        }
    }
}
