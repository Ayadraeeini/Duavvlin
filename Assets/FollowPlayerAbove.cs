using UnityEngine;

public class FollowPlayerAbove : MonoBehaviour
{
    public enum FollowMode { HardLock, CappedY }
    public FollowMode mode = FollowMode.HardLock;

    public Transform player;
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);

    [Header("Only for CappedY")]
    public float maxYSpeed = 12f; // units/second cap for vertical follow

    float initialZ;

    void Awake()
    {
        initialZ = transform.position.z; // keep Z fixed so it never disappears behind stuff
    }

    void LateUpdate()
    {
        if (!player) return;

        Vector3 target = player.position + offset;
        // Keep our Z fixed to avoid camera depth issues
        target.z = initialZ;

        if (mode == FollowMode.HardLock)
        {
            // EXACT lock — no smoothing, no bounce
            transform.position = target;
            return;
        }

        // CappedY: instant X, limited Y speed (prevents big jump spikes)
        Vector3 pos = transform.position;
        pos.x = target.x;

        float maxDeltaY = maxYSpeed * Time.deltaTime;
        pos.y = Mathf.MoveTowards(pos.y, target.y, maxDeltaY);

        pos.z = initialZ;
        transform.position = pos;
    }
}
