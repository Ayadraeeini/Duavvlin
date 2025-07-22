using UnityEngine;
using UnityEngine.UI;

public class SprayPainter : MonoBehaviour
{
    public RawImage paintArea;                     // The UI area the player can spray on
    public Image finalGraffitiDisplay;             // The wall where final spray shows
    private Texture2D paintTexture;
    public Color sprayColor = Color.green;
    public float sprayRadius = 20f;

    void Start()
    {
        // Create a blank transparent texture
        paintTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        for (int x = 0; x < paintTexture.width; x++)
        {
            for (int y = 0; y < paintTexture.height; y++)
            {
                paintTexture.SetPixel(x, y, Color.clear);
            }
        }
        paintTexture.Apply();
        paintArea.texture = paintTexture;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                paintArea.rectTransform,
                Input.mousePosition,
                null,
                out localPos
            );

            float px = (localPos.x + paintArea.rectTransform.rect.width / 2f) / paintArea.rectTransform.rect.width * paintTexture.width;
            float py = (localPos.y + paintArea.rectTransform.rect.height / 2f) / paintArea.rectTransform.rect.height * paintTexture.height;

            SprayAt((int)px, (int)py);
        }

        // Press F to finalize graffiti
        if (Input.GetKeyDown(KeyCode.F))
        {
            DisplayFinalGraffiti();
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
                    {
                        paintTexture.SetPixel(px, py, sprayColor);
                    }
                }
            }
        }
        paintTexture.Apply();
    }

    void DisplayFinalGraffiti()
    {
        // Display the final graffiti result on the wall
        finalGraffitiDisplay.sprite = Sprite.Create(
            paintTexture,
            new Rect(0, 0, paintTexture.width, paintTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        paintArea.gameObject.SetActive(false); // Hide the paint UI
    }
}
