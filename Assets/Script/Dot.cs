using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    private LineDrawer drawer;
    private Image dotImage;
    private bool isConnected = false;

    void Start()
    {
        drawer = FindObjectOfType<LineDrawer>();
        dotImage = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isConnected)
        {
            drawer.StartPath(transform);
            Connect();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isConnected && drawer.IsDrawing())
        {
            drawer.AddPoint(transform);
            Connect();
        }
    }

    void Connect()
    {
        isConnected = true;
        dotImage.color = Color.green;
    }
}
