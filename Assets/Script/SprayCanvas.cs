using UnityEngine;
using UnityEngine.UI;

public class SprayCanvas : MonoBehaviour
{
    public GameObject canvas;  // The canvas to draw on
    public RawImage drawingArea;  // The Image component that holds the drawing
    public float sprayDuration = 10f;  // How long the canvas stays visible

    private bool isDrawing = false;
    private Texture2D texture;
    private Color[] transparentPixels;

    void Start()
    {
        // Disable the canvas initially
        canvas.SetActive(false);

        // Create a texture for drawing (default size 512x512)
        texture = new Texture2D(512, 512);
        transparentPixels = new Color[texture.width * texture.height];

        // Set all initial pixels to be transparent (making the canvas empty)
        for (int i = 0; i < transparentPixels.Length; i++)
        {
            transparentPixels[i] = Color.clear;
        }

        texture.SetPixels(transparentPixels);
        texture.Apply();

        // Apply the texture to the drawing area (RawImage)
        drawingArea.texture = texture;
    }

    void Update()
    {
        // If the canvas is active, allow the player to draw
        if (isDrawing)
        {
            DrawOnCanvas();
        }
    }

    public void StartDrawing()
    {
        // Activate the canvas to show the drawing area
        canvas.SetActive(true);
        isDrawing = true;

        // Start a timer to stop drawing after the duration
        Invoke("StopDrawing", sprayDuration);
    }

    void DrawOnCanvas()
    {
        Vector2 mousePos = Input.mousePosition;

        // Convert the mouse position to canvas coordinates
        RectTransform rt = drawingArea.GetComponent<RectTransform>();
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mousePos, null, out localPos);

        // Calculate the pixel coordinates in the texture
        int x = Mathf.FloorToInt((localPos.x / rt.rect.width) * texture.width);
        int y = Mathf.FloorToInt((localPos.y / rt.rect.height) * texture.height);

        // Check if the mouse is inside the drawing area
        if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
        {
            // Set the pixel color (spray effect, e.g., black spray)
            texture.SetPixel(x, y, Color.black);  // Change to any color you want to "spray"
            texture.Apply();
        }
    }

    // Make StopDrawing public so it can be accessed from other scripts
    public void StopDrawing()
    {
        // Deactivate the canvas after the spray duration
        canvas.SetActive(false);
        isDrawing = false;
    }
}
