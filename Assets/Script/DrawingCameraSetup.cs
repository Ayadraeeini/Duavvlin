using UnityEngine;

public class DrawingCameraSetup : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Color backgroundColor = Color.white;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = gameObject.AddComponent<Camera>();

        cam.targetTexture = renderTexture;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;
        cam.orthographic = true;
        cam.orthographicSize = 5f; // Adjust based on your needs
    }
}