using UnityEngine;

public class PaperFloat : MonoBehaviour
{
    public float minSpeed = 2f;
    public float maxSpeed = 4f;
    public float minSway = 0.3f;
    public float maxSway = 0.7f;

    private float speed;
    private float swayAmount;
    private float swaySpeed;
    private float startY;
    private float twistSpeed;

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        swayAmount = Random.Range(minSway, maxSway);
        swaySpeed = Random.Range(2f, 4f);
        twistSpeed = Random.Range(15f, 40f);
        startY = transform.position.y;
    }

    void Update()
    {
        // Move to the right
        transform.position += Vector3.right * speed * Time.deltaTime;

        // Wiggle up and down
        float newY = startY + Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Spin slowly on Z (visible 2D spin)
        transform.Rotate(0f, 0f, twistSpeed * Time.deltaTime);

        // Optional destroy when far off screen
        if (transform.position.x > Camera.main.transform.position.x + 20f)
        {
            Destroy(gameObject);
        }
    }
}
