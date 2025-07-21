using UnityEngine;
using UnityEngine.UI;

public class SprayPainter : MonoBehaviour
{
    public RawImage paintArea;
    public RawImage paintArea2;
    private Texture2D paintTexture;
    public Color sprayColor = Color.green;
    public float sprayRadius = 10f;

    void Start()
    {
        paintTexture = new Texture2D(512, 512);
        for (int x = 0; x < paintTexture.width; x++)
        {
            for (int y = 0; y < paintTexture.height; y++)
                paintTexture.SetPixel(x, y, Color.clear);
        }
        paintTexture.Apply();
        paintArea.texture = paintTexture;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // If mouse is clicked
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                paintArea.rectTransform,
                Input.mousePosition,
                null,
                out localPos
            );

            float px = (localPos.x + paintArea.rectTransform.rect.width / 2) / paintArea.rectTransform.rect.width * paintTexture.width;
            float py = (localPos.y + paintArea.rectTransform.rect.height / 2) / paintArea.rectTransform.rect.height * paintTexture.height;

            SprayAt((int)px, (int)py);
        }
    }

    void SprayAt(int x, int y)
    {
        for (int i = -Mathf.RoundToInt(sprayRadius); i <= sprayRadius; i++)
        {
            for (int j = -Mathf.RoundToInt(sprayRadius); j <= sprayRadius; j++)
            {
                if (i * i + j * j <= sprayRadius * sprayRadius)
                {
                    int px = x + i;
                    int py = y + j;
                    if (px >= 0 && px < paintTexture.width && py >= 0 && py < paintTexture.height)
                        paintTexture.SetPixel(px, py, sprayColor);
                }
            }
        }
        paintTexture.Apply();
    }
}
