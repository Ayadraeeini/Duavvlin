using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private bool drawing = false;

    void Update()
    {
        if (drawing)
        {
            Vector3 pos = Input.mousePosition;
            pos.z = 0;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, pos);
        }
    }

    public void StartPath(Transform startDot)
    {
        points.Clear();
        lineRenderer.positionCount = 0;
        Vector3 pos = RectTransformToScreen(startDot.GetComponent<RectTransform>());
        AddPointToLine(pos);
        drawing = true;
    }

    public void AddPoint(Transform newDot)
    {
        Vector3 pos = RectTransformToScreen(newDot.GetComponent<RectTransform>());
        AddPointToLine(pos);
    }

    public bool IsDrawing() => drawing;

    private void AddPointToLine(Vector3 pos)
    {
        points.Add(pos);
        lineRenderer.positionCount = points.Count + 1;
        for (int i = 0; i < points.Count; i++)
            lineRenderer.SetPosition(i, points[i]);
        lineRenderer.SetPosition(points.Count, Input.mousePosition);
    }

    Vector3 RectTransformToScreen(RectTransform rt)
    {
        Vector3[] worldCorners = new Vector3[4];
        rt.GetWorldCorners(worldCorners);
        return (worldCorners[0] + worldCorners[2]) / 2f;
    }
}
