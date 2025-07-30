using UnityEngine;
using UnityEngine.UI;

public class DrawOnCanvas : MonoBehaviour
{
    public Camera m_camera;      // Reference to the camera
    public RawImage drawingArea; // The area where the player draws (RawImage)
    public Texture2D texture;    // Texture where the drawing will be stored
    public Color drawColor = Color.black;  // Drawing color, default black

    private bool isDrawing = false;
    private Vector2 lastPosition;
    private bool isNearSprayBox = false;  // Check if the player is near the SprayBox

    // Start is called before the first frame update
    void Start()
    {
        // Initially disable the canvas and texture
        drawingArea.gameObject.SetActive(false); // Hide drawing area initially
        Debug.Log("Canvas hidden initially.");

        // Create a new Texture2D that matches the size of the RawImage
        texture = new Texture2D(512, 512);  // Size should match RawImage size
        texture.filterMode = FilterMode.Bilinear;  // Smooth the texture if scaled
        texture.wrapMode = TextureWrapMode.Repeat; // Allow texture to repeat if needed

        // Assign the texture to the RawImage
        drawingArea.texture = texture;
    }

    void Update()
    {
        // Check if the player is pressing E and is near the SprayBox
        if (isNearSprayBox && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed - Starting drawing.");
            StartDrawing();  // Start drawing if the player is near the SprayBox
        }

        if (isDrawing)
        {
            DrawOnArea();
        }
    }

    // Start drawing
    public void StartDrawing()
    {
        drawingArea.gameObject.SetActive(true);  // Show the drawing area (RawImage)
        isDrawing = true;
        lastPosition = m_camera.ScreenToWorldPoint(Input.mousePosition); // Store initial position
        Debug.Log("Drawing started.");
    }

    // Stop drawing
    public void StopDrawing()
    {
        drawingArea.gameObject.SetActive(false);  // Deactivate the drawing area (RawImage)
        isDrawing = false;
        SaveDrawing();  // Optionally save or apply the drawing
        Debug.Log("Drawing stopped.");
    }

    // Draw on the RawImage canvas
    void DrawOnArea()
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);  // Convert mouse position to world space

        // If the mouse has moved, draw a line between the last and current position
        if (lastPosition != (Vector2)mousePos)
        {
            DrawLine(lastPosition, mousePos);  // Draw the line on the texture
            lastPosition = mousePos;  // Update last position to the current mouse position
        }
    }

    // Draw a line between two points on the texture
    void DrawLine(Vector2 start, Vector2 end)
    {
        // Convert world space coordinates to texture coordinates
        Vector2 startTexture = WorldToTexture(start);
        Vector2 endTexture = WorldToTexture(end);

        // Draw a line on the texture between the start and end points
        float distance = Vector2.Distance(startTexture, endTexture);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i < steps; i++)
        {
            Vector2 point = Vector2.Lerp(startTexture, endTexture, i / (float)steps);
            texture.SetPixel((int)point.x, (int)point.y, drawColor);  // Draw on the texture (set pixel color)
        }

        texture.Apply();  // Apply the changes to the texture
    }

    // Convert world position to texture position
    Vector2 WorldToTexture(Vector2 worldPosition)
    {
        // Normalize the world position to the texture coordinates (0-1 range)
        Vector2 texturePos = worldPosition / new Vector2(drawingArea.rectTransform.rect.width, drawingArea.rectTransform.rect.height);
        return texturePos * new Vector2(texture.width, texture.height);  // Scale to texture resolution
    }

    // Optionally save the drawing (this is a placeholder for saving the texture to a file or elsewhere)
    void SaveDrawing()
    {
        // You can implement a method to save the drawing if needed, or export it.
        // Example: Save the texture as a PNG file:
        // byte[] bytes = texture.EncodeToPNG();
        // System.IO.File.WriteAllBytes("Drawing.png", bytes);
    }

    // Trigger when the player enters the SprayBox area
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SprayBox"))
        {
            isNearSprayBox = true; // Player is near the SprayBox
            Debug.Log("Player is near SprayBox");
        }
    }

    // Trigger when the player exits the SprayBox area
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("SprayBox"))
        {
            isNearSprayBox = false; // Player left the SprayBox area
            Debug.Log("Player left SprayBox");
        }
    }
}
