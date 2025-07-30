using UnityEngine;

public class PlaceDrawing : MonoBehaviour
{
    public GameObject drawingPrefab;  // Prefab with SpriteRenderer to display the drawing

    void Start()
    {
        // Retrieve the saved position where "E" was pressed
        float x = PlayerPrefs.GetFloat("DrawingPosX", 0);
        float y = PlayerPrefs.GetFloat("DrawingPosY", 0);
        float z = PlayerPrefs.GetFloat("DrawingPosZ", 0);
        Vector3 interactionPoint = new Vector3(x, y, z);

        // Retrieve the saved drawing texture from PlayerPrefs
        string base64String = PlayerPrefs.GetString("DrawingData", "");
        if (!string.IsNullOrEmpty(base64String))
        {
            byte[] data = System.Convert.FromBase64String(base64String);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);  // Load the saved texture

            // Instantiate the drawing prefab at the saved position and apply the drawing texture
            GameObject drawingObject = Instantiate(drawingPrefab, interactionPoint, Quaternion.identity);
            SpriteRenderer spriteRenderer = drawingObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
