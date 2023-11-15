using System;
using ChartAndGraph;
using UnityEngine;

[RequireComponent(typeof(CustomChartPointer))]
public class GraphRectZoom : MonoBehaviour
{
    public RectTransform Marker;
    public int MinMarkerPixels = 20;
    private bool mIsDown;
    private bool mIsUp = true;
    private bool mIsZoomed;
    private CustomChartPointer mPointer;
    private RectTransform mRect;

    private Vector2 mStart;

    // Start is called before the first frame update
    private void Start()
    {
        mRect = GetComponent<RectTransform>();
        mPointer = GetComponent<CustomChartPointer>();
        Marker.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (mIsZoomed && mPointer.IsMouseDown)
        {
            mIsZoomed = false;
            var graph = GetComponent<GraphChart>();
            graph.DataSource.AutomaticHorizontalView = true;
            graph.DataSource.AutomaticVerticallView = true;
            graph.HorizontalScrolling = 0;
            graph.VerticalScrolling = 0;
            mIsUp = false;
            return;
        }

        if (mIsUp == false && mPointer.IsMouseDown)
            return;
        mIsUp = true;
        var pointerPos = mPointer.ScreenPosition;

        if (mIsDown == false)
        {
            if (mPointer.IsMouseDown) // the mouse is clicked in this frame
            {
                OnMarkerStart(); //start the marker
                OnMarkerMove();
            }
        }
        else
        {
            if (mPointer.IsMouseDown == false) // the mouse is up this frame
            {
                OnMarkerEnd(); // end the marker
                if (mPointer.IsOut == false)
                    SetGraphZoom(); // set graph zoom
            }
            else
            {
                OnMarkerMove(); // the mouse is being held
            }
        }
    }

    private void OnMarkerStart()
    {
        mIsDown = true;
        mStart = mPointer.ScreenPosition;
        Marker.gameObject.SetActive(true);
    }

    private void OnMarkerMove()
    {
        Vector3 end = mPointer.ScreenPosition;
        var min = new Vector3(Mathf.Min(mStart.x, end.x), Mathf.Min(mStart.y, end.y));
        var max = new Vector3(Mathf.Max(mStart.x, end.x), Mathf.Max(mStart.y, end.y));
        Marker.position = min;
        Marker.sizeDelta = max - min;
    }

    private void OnMarkerEnd()
    {
        mIsDown = false;
        Marker.gameObject.SetActive(false);
    }

    private bool CheckBounds()
    {
        var v = mPointer.ScreenPosition - mStart;
        if (Math.Abs(v.x) < MinMarkerPixels || Math.Abs(v.y) < MinMarkerPixels)
            return false;
        return true;
    }

    private void SetGraphZoom()
    {
        if (CheckBounds() == false)
            return;
        var graph = GetComponent<GraphChart>();
        double x1, y1;
        double x2, y2;
        if (graph.PointToClient(mStart, out x1, out y1))
            if (graph.PointToClient(mPointer.ScreenPosition, out x2, out y2))
            {
                mIsZoomed = true;
                graph.DataSource.AutomaticHorizontalView = false;
                graph.DataSource.AutomaticVerticallView = false;

                var min = new DoubleVector2(Math.Min(x1, x2), Math.Min(y1, y2));
                var max = new DoubleVector2(Math.Max(x1, x2), Math.Max(y1, y2));
                graph.HorizontalScrolling = min.x;
                graph.VerticalScrolling = min.y;
                graph.DataSource.HorizontalViewSize = max.x - min.x;
                graph.DataSource.VerticalViewSize = max.y - min.y;
                graph.DataSource.HorizontalViewOrigin = 0;
                graph.DataSource.VerticalViewOrigin = 0;
            }
    }
}