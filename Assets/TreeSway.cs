using UnityEngine;

public class TreeSway : MonoBehaviour
{
    public float swayAngle = 2f;     // Max angle in degrees
    public float swaySpeed = 1f;     // Speed of swaying

    private float initialZRotation;

    void Start()
    {
        initialZRotation = transform.eulerAngles.z;
    }

    void Update()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.z = initialZRotation + sway;
        transform.eulerAngles = currentRotation;
    }
}
