using UnityEngine;
using UnityEngine.UI;

public class SprayPainter : MonoBehaviour
{
    public RawImage sprayArea; // Assign the UI image used for drawing
    private Texture2D sprayTexture;
    private bool isDrawing;

    void Start()
    {
        sprayTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        sprayTexture.filterMode = FilterMode.Point;

        // Fill with transparent
        Color[] clearPixels = new Color[512 * 512];
        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = new Color(0, 0, 0, 0);

        sprayTexture.SetPixels(clearPixels);
        sprayTexture.Apply();

        sprayArea.texture = sprayTexture;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // Left mouse drag
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sprayArea.rectTransform,
                Input.mousePosition,
                null,
                out localPos);

            Rect rect = sprayArea.rectTransform.rect;

            // Normalize 0-1, then scale to texture size
            float x = Mathf.InverseLerp(rect.xMin, rect.xMax, localPos.x);
            float y = Mathf.InverseLerp(rect.yMin, rect.yMax, localPos.y);

            int px = Mathf.FloorToInt(x * sprayTexture.width);
            int py = Mathf.FloorToInt(y * sprayTexture.height);

            DrawDot(px, py, Color.green);
        }
    }

    void DrawDot(int x, int y, Color color)
    {
        int size = 8;
        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                int px = Mathf.Clamp(x + dx, 0, sprayTexture.width - 1);
                int py = Mathf.Clamp(y + dy, 0, sprayTexture.height - 1);
                sprayTexture.SetPixel(px, py, color);
            }
        }
        sprayTexture.Apply();
    }

    public Texture2D GetSprayResult()
    {
        return sprayTexture;
    }
}
