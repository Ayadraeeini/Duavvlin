using UnityEngine;

public class FakeFlashingLight : MonoBehaviour
{
    public float flashInterval = 0.5f;
    private SpriteRenderer sr;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= flashInterval)
        {
            sr.enabled = !sr.enabled; // turn the circle on/off
            timer = 0f;
        }
    }
}
