using UnityEngine;

public class SprayBox : MonoBehaviour
{
    public string outlineImageName = "outline1";
    public string finalImageName = "final1";

    private SpriteRenderer outlineRenderer;
    private SpriteRenderer finalRenderer;
    private SpriteRenderer baseRenderer; // The one on the SprayBox object itself
    private Collider2D boxCollider;

    private int outlinePresses = 0;
    private int finalPresses = 0;
    private const int maxPresses = 4;

     public Vector3 finalScale = new Vector3(0.81f, 0.66f, 0.87f);
     public Vector3 outlineScale = new Vector3(0.48f, 0.396f, 0.52f);
    public GameObject completionEffectPrefab; 

    void Start()
    {
        // Try get base SpriteRenderer and Collider if any
        baseRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        // Load textures
        Texture2D outlineTex = Resources.Load<Texture2D>(outlineImageName);
        Texture2D finalTex = Resources.Load<Texture2D>(finalImageName);

        if (outlineTex == null || finalTex == null)
        {
            Debug.LogError("Textures not found! Check Resources folder and file names.");
            return;
        }

        // Create outline
        GameObject outlineGO = new GameObject("Outline");
        outlineGO.transform.SetParent(transform);
        outlineGO.transform.localPosition = Vector3.zero;
        outlineGO.transform.localScale = outlineScale;
        outlineRenderer = outlineGO.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = Sprite.Create(outlineTex, new Rect(0, 0, outlineTex.width, outlineTex.height), new Vector2(0.5f, 0.5f));
        outlineRenderer.color = new Color(1, 1, 1, 0f);

        // Create final
        GameObject finalGO = new GameObject("Final");
        finalGO.transform.SetParent(transform);
        finalGO.transform.localPosition = Vector3.zero;
        finalGO.transform.localScale = finalScale;
        finalRenderer = finalGO.AddComponent<SpriteRenderer>();
        finalRenderer.sprite = Sprite.Create(finalTex, new Rect(0, 0, finalTex.width, finalTex.height), new Vector2(0.5f, 0.5f));
        finalRenderer.color = new Color(1, 1, 1, 0f);
    }

    public void Paint()
    {
        if (outlineRenderer == null || finalRenderer == null) return;

        if (outlinePresses < maxPresses)
        {
            outlinePresses++;
            float alpha = outlinePresses / (float)maxPresses;
            outlineRenderer.color = new Color(1, 1, 1, alpha);
        }
        else if (finalPresses < maxPresses)
        {
            finalPresses++;
            float alpha = finalPresses / (float)maxPresses;
            finalRenderer.color = new Color(1, 1, 1, alpha);

            if (finalPresses == maxPresses)
            {
                HideBaseBox();
            }
        }
    }

    private void HideBaseBox()
    {
        if (baseRenderer != null)
            baseRenderer.enabled = false;

        if (boxCollider != null)
            boxCollider.enabled = false;

        Debug.Log("[SprayBox] Graffiti complete — base box hidden.");

        if (completionEffectPrefab != null)
        {
            Instantiate(completionEffectPrefab, transform.position, Quaternion.identity);
            Debug.Log("[SprayBox] Particle effect played.");
        }

    }
}
