using UnityEngine;

public class MoonBehavior : MonoBehaviour
{
    public float floatStrength = 0.1f;  // How high it floats
    public float floatSpeed = 1f;       // How fast it floats up/down
    public float moveSpeed = 0.2f;      // Optional horizontal movement

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatStrength;
        transform.position = startPos + new Vector3(Time.time * moveSpeed, yOffset, 0);
    }
}
