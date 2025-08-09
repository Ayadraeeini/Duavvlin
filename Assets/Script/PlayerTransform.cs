using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite transformedSprite;

    private SpriteRenderer sr;
    private Animator animator;
    private bool isTransformed = false;
    private float duration = 5f;
    private Vector3 originalScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;

        if (normalSprite != null)
            sr.sprite = normalSprite;

        if (animator == null)
            Debug.LogWarning("Animator is missing from Player!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isTransformed)
        {
            Debug.Log("T key pressed - attempting to transform.");
            StartCoroutine(TransformSprite());
        }
    }

    private System.Collections.IEnumerator TransformSprite()
    {
        isTransformed = true;

        // Trigger animation to can
        if (animator != null)
        {
            animator.SetTrigger("ToCan");
            Debug.Log("Triggered: ToCan");
        }

        // Shrink scale
        yield return StartCoroutine(ScaleOverTime(transform, transform.localScale, new Vector3(0.5f, 0.5f, 1f), 0.5f));

        // Swap sprite
        if (transformedSprite != null)
        {
            sr.sprite = transformedSprite;
            Debug.Log("Sprite transformed!");
        }

        yield return new WaitForSeconds(duration);

        // Back to normal
        if (animator != null)
        {
            animator.SetTrigger("ToHuman");
            Debug.Log("Triggered: ToHuman");
        }

        // Scale back to original
        yield return StartCoroutine(ScaleOverTime(transform, transform.localScale, originalScale, 0.5f));

        if (normalSprite != null)
        {
            sr.sprite = normalSprite;
            Debug.Log("Reverted to normal.");
        }

        isTransformed = false;
    }

    private System.Collections.IEnumerator ScaleOverTime(Transform obj, Vector3 from, Vector3 to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            obj.localScale = Vector3.Lerp(from, to, t / time);
            yield return null;
        }
        obj.localScale = to;
    }

    // Public getter
    public bool IsTransformed() => isTransformed;
}
