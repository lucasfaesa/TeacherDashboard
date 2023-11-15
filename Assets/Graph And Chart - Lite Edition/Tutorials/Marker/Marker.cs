using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

public class Marker : MonoBehaviour
{
    public GraphChartBase Chart;
    public RectTransform LastPoint;
    public RectTransform Area;
    public Text MouseText;

    private DoubleRect currentRect;

    // Use this for initialization
    private void Start()
    {
        if (Chart != null) Chart.OnRedraw.AddListener(Redraw);
    }

    // Update is called once per frame
    private void Update()
    {
        double mx, my;
        if (MouseText != null)
        {
            if (Chart.MouseToClient(out mx, out my)) MouseText.text = string.Format("{0:0.00} , {1:0.00}", mx, my);
        }
        else
        {
            MouseText.text = "";
        }
    }

    private void Redraw()
    {
        if (Chart == null)
            return;

        if (Chart.IsRectVisible(currentRect) == false)
        {
            double endX = (float)(Chart.HorizontalScrolling + Chart.DataSource.HorizontalViewSize);
            var x = endX - 1f;
            double y = (float)Chart.VerticalScrolling;
            double endY = (float)Chart.DataSource.GetMaxValue(1, false);
            currentRect = new DoubleRect(x, y, endX - x, endY - y);
        }

        DoubleRect trimRect;
        if (Chart.TrimRect(currentRect, out trimRect)) Chart.RectToCanvas(Area, trimRect);


        DoubleVector3 last;
        if (Chart.DataSource.GetLastPoint("Player 1", out last))
        {
            Vector3 pos;
            if (Chart.PointToWorldSpace(out pos, last.x, last.y, "Player 1"))
                if (LastPoint != null)
                    LastPoint.transform.position = pos;
        }
    }
}