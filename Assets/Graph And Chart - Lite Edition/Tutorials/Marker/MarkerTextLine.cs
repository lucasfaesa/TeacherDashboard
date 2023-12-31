﻿using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

public class MarkerTextLine : MonoBehaviour
{
    public GraphChartBase Chart;
    public Text TextObject;

    /// <summary>
    ///     the first line position in graph coordinates
    /// </summary>
    public DoubleVector2 point1;

    /// <summary>
    ///     the second line position in graph coordinates
    /// </summary>
    public DoubleVector2 point2;

    /// <summary>
    ///     offsets the text on the x axis away from the selected point
    /// </summary>
    public float SeperationOffset;

    /// <summary>
    ///     offsets the text on the y axus away from the selected point
    /// </summary>
    public float ElevationOffset;

    public bool Rotate = true;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Chart == null)
            return;
        Vector3 res1, res2;
        if (Chart.PointToWorldSpace(out res1, point1.x, point1.y))
            if (Chart.PointToWorldSpace(out res2, point2.x, point2.y))
            {
                Vector2 finalPos = (res2 + res1) * 0.5f;
                Vector2 seperationVector = (res2 - res1).normalized;
                var elevationVector = ChartCommon.Perpendicular(seperationVector);
                TextObject.transform.position =
                    finalPos + SeperationOffset * seperationVector + ElevationOffset * elevationVector;
                if (Rotate)
                    TextObject.transform.localRotation = Quaternion.Euler(0f, 0f,
                        Mathf.Rad2Deg * Mathf.Atan2(seperationVector.y, seperationVector.x));
            }
    }
}