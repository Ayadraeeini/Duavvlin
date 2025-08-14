using UnityEngine;

public class SpinAndFade : MonoBehaviour
{
    public Transform player;
    public float triggerDistance = 3f;
    public float fadeSpeed = 1f;

    private bool hasSpun = false;
    private float rotationAmount = 0f;
    private SpriteRenderer sr;
    private float targetAlpha = 0f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        SetAlpha(0f); // start invisible
    }

    void Update()
    {
        float distance = Vector2.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            targetAlpha = 1f;

            if (!hasSpun)
                StartCoroutine(RotateWithEase());
        }
        else
        {
            targetAlpha = 0f;
        }

        // Smooth fade
        Color c = sr.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        sr.color = c;
    }

    System.Collections.IEnumerator RotateWithEase()
    {
        hasSpun = true;
        float totalRotation = 0f;
        float duration = 2f; // total time of spin
        float elapsed = 0f;

        while (totalRotation < 720f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float speed = Mathf.Lerp(360f, 0f, t); // ease out

            float rotateThisFrame = speed * Time.deltaTime;
            transform.Rotate(0, 0, rotateThisFrame);

            totalRotation += rotateThisFrame;
            yield return null;
        }
    }

    void SetAlpha(float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
