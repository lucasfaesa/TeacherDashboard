using System;
using ChartAndGraph;
using UnityEngine;

/// <summary>
///     this is an example of zoom using mouse for the graph chart
/// </summary>
[RequireComponent(typeof(GraphChart))]
public class GraphZoom : MonoBehaviour
{
    public float errorMargin = 5f;
    public float ZoomSpeed = 20f;
    public float MaxViewSize = 10f;
    public float MinViewSize = 0.1f;
    private GraphChart graph;
    private DoubleVector3 InitalScrolling;
    private DoubleVector3 InitalViewDirection;
    private DoubleVector3 InitalViewSize;
    private DoubleVector3 InitialOrigin;
    private DoubleVector3 mZoomBaseChartSpace;
    private Vector3 mZoomBasePosition;
    private float totalZoom;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (graph == null) // no graph attached to this script for some reason
            return;
        Vector2 mousePos = Input.mousePosition;
        double mouseX, mouseY;
        graph.PointToClient(mousePos, out mouseX, out mouseY);
        if (CompareWithError(mousePos, mZoomBasePosition) == false) // the mouse has moved beyond the erroo
        {
            mZoomBasePosition = mousePos;
            graph.PointToClient(mousePos, out mouseX, out mouseY);
            mZoomBaseChartSpace = new DoubleVector3(mouseX, mouseY);
            ResetZoomAnchor();
        }
        else
        {
            mousePos = mZoomBasePosition;
        }

        var delta = Input.mouseScrollDelta.y;
        totalZoom += delta; //accumilate the delta change for the currnet positions

        if (delta != 0 && graph.PointToClient(mousePos, out mouseX, out mouseY))
        {
            var ViewCenter = InitialOrigin + InitalScrolling;
            var trans = new DoubleVector3(mZoomBaseChartSpace.x - ViewCenter.x, mZoomBaseChartSpace.y - ViewCenter.y);
            var growFactor = Mathf.Pow(2, totalZoom / ZoomSpeed);
            var hSize = InitalViewSize.x * growFactor;
            var vSize = InitalViewSize.y * growFactor;
            if (hSize * InitalViewDirection.x < MaxViewSize && hSize * InitalViewDirection.x > MinViewSize &&
                vSize * InitalViewDirection.y < MaxViewSize && vSize * InitalViewDirection.y > MinViewSize)
            {
                graph.HorizontalScrolling = InitalScrolling.x + trans.x - trans.x * growFactor;
                graph.VerticalScrolling = InitalScrolling.y + trans.y - trans.y * growFactor;
                graph.DataSource.HorizontalViewSize = hSize;
                graph.DataSource.VerticalViewSize = vSize;
            }
        }
    }

    private void OnEnable()
    {
        graph = GetComponent<GraphChart>();
    }

    private void ResetZoomAnchor()
    {
        totalZoom = 0;
        InitalScrolling = new DoubleVector3(graph.HorizontalScrolling, graph.VerticalScrolling);
        InitalViewSize = new DoubleVector3(graph.DataSource.HorizontalViewSize, graph.DataSource.VerticalViewSize);
        InitalViewDirection = new DoubleVector3(Math.Sign(InitalViewSize.x), Math.Sign(InitalViewSize.y));
        InitialOrigin = new DoubleVector3(graph.DataSource.HorizontalViewOrigin, graph.DataSource.VerticalViewOrigin);
    }

    private bool CompareWithError(Vector3 a, Vector3 b)
    {
        if (Mathf.Abs(a.x - b.x) > errorMargin)
            return false;
        if (Mathf.Abs(a.y - b.y) > errorMargin)
            return false;
        return true;
    }
}