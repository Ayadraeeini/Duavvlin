using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    [Header("Door References")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Target Positions")]
    public Vector3 leftOpenPosition = new Vector3(9.34f, 0f, 0f);
    public Vector3 rightOpenPosition = new Vector3(9.78f, 0f, 0f);

    [Header("Settings")]
    public float moveSpeed = 3f;
    public string playerTag = "Player";

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            triggered = true;
            Debug.Log("Gate triggered by player");
        }
    }

    void Update()
    {
        if (!triggered) return;

        if (leftDoor != null)
            leftDoor.position = Vector3.MoveTowards(leftDoor.position, leftOpenPosition, moveSpeed * Time.deltaTime);

        if (rightDoor != null)
            rightDoor.position = Vector3.MoveTowards(rightDoor.position, rightOpenPosition, moveSpeed * Time.deltaTime);
    }
}
