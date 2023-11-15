using System;
using System.Collections;
using ChartAndGraph;
using UnityEngine;
using Random = UnityEngine.Random;

public class GraphChartFeed : MonoBehaviour
{
    private void Start()
    {
        var graph = GetComponent<GraphChartBase>();
        if (graph != null)
        {
            graph.Scrollable = false;
            graph.HorizontalValueToStringMap[0.0] = "Zero"; // example of how to set custom axis strings
            graph.DataSource.StartBatch();
            graph.DataSource.ClearCategory("Player 1");
            //graph.DataSource.ClearAndMakeBezierCurve("Player 2");

            for (var i = 0; i < 5; i++)
            {
                graph.DataSource.AddPointToCategory("Player 1", DateTime.Now + TimeSpan.FromDays(i), Random.value * 10f + 20f);
                /*if (i == 0)
                    graph.DataSource.SetCurveInitialPoint("Player 2", i * 5, Random.value * 10f + 10f);
                else
                    graph.DataSource.AddLinearCurveToCategory("Player 2",
                        new DoubleVector2(i * 5, Random.value * 10f + 10f));*/
            }

            //graph.DataSource.MakeCurveCategorySmooth("Player 2");
            graph.DataSource.EndBatch();
        }
        // StartCoroutine(ClearAll());
    }

    private IEnumerator ClearAll()
    {
        yield return new WaitForSeconds(5f);
        var graph = GetComponent<GraphChartBase>();

        graph.DataSource.Clear();
    }
}