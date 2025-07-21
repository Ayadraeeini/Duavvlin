using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite transformedSprite;

    private SpriteRenderer sr;
    private bool isTransformed = false;
    private float duration = 5f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (normalSprite != null)
            sr.sprite = normalSprite;
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

        if (transformedSprite != null)
        {
            sr.sprite = transformedSprite;
            Debug.Log("Sprite transformed!");
        }

        yield return new WaitForSeconds(duration);

        if (normalSprite != null)
        {
            sr.sprite = normalSprite;
            Debug.Log("Reverted to normal.");
        }

        isTransformed = false;
    }
}
