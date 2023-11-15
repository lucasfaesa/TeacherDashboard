using System;
using ChartAndGraph;
using UnityEngine;

public class TrackerLine : MonoBehaviour
{
    public GraphChartBase Chart;
    public RectTransform Area;
    public float lineThickness = 1.0f;
    public bool TrackLast = true;
    public float yPosition;

    // Use this for initialization
    private void Start()
    {
        if (Chart != null) Chart.OnRedraw.AddListener(Redraw);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void Redraw()
    {
        if (Chart == null)
            return;
        DoubleVector3 last;
        if (Chart.DataSource.GetLastPoint("Player 1", out last))
        {
            if (TrackLast == false)
                last.y = yPosition;
            var rect = new DoubleRect(last.x - Chart.DataSource.HorizontalViewSize, last.y - lineThickness * 0.5f,
                Chart.DataSource.HorizontalViewSize, lineThickness);
            DoubleRect trimRect;

            if (Area != null)
            {
                Vector3 res;
                if (Chart.PointToWorldSpace(out res, rect.min.x, rect.min.y))
                {
                    res.y += lineThickness;
                    double x, y;
                    if (Chart.PointToClient(res, out x, out y))
                    {
                        var thick = Math.Abs(rect.min.y - y);
                        rect = new DoubleRect(last.x - Chart.DataSource.HorizontalViewSize, last.y - thick * 0.5f,
                            Chart.DataSource.HorizontalViewSize, thick);
                    }
                }

                if (Chart.TrimRect(rect, out trimRect)) // if the rect is in display
                {
                    if (Area.gameObject.activeSelf == false) // we draw it
                        Area.gameObject.SetActive(true);
                    Chart.RectToCanvas(Area, trimRect);
                    //       Area.sizeDelta = new Vector2(Area.sizeDelta.x, lineThickness);
                }
                else
                {
                    if (Area.gameObject.activeSelf) // otherwise it is set as incative
                        Area.gameObject.SetActive(false);
                }
            }
        }
    }
}