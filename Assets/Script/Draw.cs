using UnityEngine;

public class Draw : MonoBehaviour
{
    [Header("Canvas Settings")]
    public GameObject sprayCanvas; // Reference to the canvas for future use

    // Start is called before the first frame update
    void Start()
    {
        if (sprayCanvas != null)
        {
            // Ensure that the canvas is initially hidden
            sprayCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Canvas is not assigned!");
        }
    }

    // This method starts the drawing process (can be called later)
    public void StartDrawing()
    {
        if (sprayCanvas != null)
        {
            sprayCanvas.SetActive(true);  // Activate the canvas if it's assigned
        }
        else
        {
            Debug.LogError("Canvas not assigned! Cannot start drawing.");
        }
    }

    // This method stops the drawing process
    public void StopDrawing()
    {
        if (sprayCanvas != null)
        {
            sprayCanvas.SetActive(false);  // Deactivate the canvas when done
        }
        else
        {
            Debug.LogError("Canvas not assigned! Cannot stop drawing.");
        }
    }

    // This method could be triggered when the player interacts with the spray box
    public void OnSprayBoxInteraction()
    {
        StartDrawing();  // Activate the canvas (you can add more logic later)
    }

    // This method could be triggered when the player finishes the drawing or presses 'E'
    public void OnFinishDrawing()
    {
        StopDrawing();  // Deactivate the canvas (can add more actions later)
    }
}
