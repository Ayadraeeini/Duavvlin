using UnityEngine;
using System.Collections.Generic;

public class DrawOnSprayBox : MonoBehaviour
{
    [Header("Drawing Settings")]
    public float lineWidth = 0.1f; // Line width of the drawing
    private List<Vector2> points;  // List to hold the points drawn
    private LineRenderer lineRenderer; // Reference to LineRenderer

    private bool isDrawing = false; // Check if drawing is active
    private Vector2 lastPosition;  // Keep track of the last drawing position

    void Start()
    {
        points = new List<Vector2>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (isDrawing)
        {
            // Use arrow keys for movement and drawing
            MoveAndDraw();
        }
    }

    // Start drawing when movement is locked
    public void StartDrawing(Vector2 initialPosition)
    {
        isDrawing = true;
        lastPosition = initialPosition;
        points.Clear();
        points.Add(lastPosition);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(0, lastPosition);
    }

    // Draw lines as player moves
    private void MoveAndDraw()
    {
        // Get the player's current position relative to the spray box
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);

        // If the player moved a significant distance from the last position, draw a new point
        if (Vector2.Distance(currentPosition, lastPosition) > lineWidth)
        {
            points.Add(currentPosition);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPosition(points.Count - 1, currentPosition);
            lastPosition = currentPosition;
        }
    }

    // Stop drawing when player exits
    public void StopDrawing()
    {
        isDrawing = false;
    }
}
