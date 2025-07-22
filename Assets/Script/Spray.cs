using UnityEngine;

public class Spray : MonoBehaviour
{
    public GameObject graffitiUI;
    public SprayPainter sprayPainter; // Assign SprayPainter script
    public SpriteRenderer graffitiWallRenderer; // Assign the wall SpriteRenderer
    private bool inRange;

    void Start()
    {
        graffitiUI.SetActive(false);
    }

    void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            graffitiUI.SetActive(true); // Open UI
        }

        if (graffitiUI.activeSelf && Input.GetKeyDown(KeyCode.Return)) // Finish
        {
            graffitiUI.SetActive(false);
            ApplySprayToWall();
        }
    }

    void ApplySprayToWall()
    {
        Texture2D sprayResult = sprayPainter.GetSprayResult();

        // Convert Texture2D to Sprite
        Rect rect = new Rect(0, 0, sprayResult.width, sprayResult.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite newSprite = Sprite.Create(sprayResult, rect, pivot);

        graffitiWallRenderer.sprite = newSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            graffitiUI.SetActive(false);
        }
    }
}
