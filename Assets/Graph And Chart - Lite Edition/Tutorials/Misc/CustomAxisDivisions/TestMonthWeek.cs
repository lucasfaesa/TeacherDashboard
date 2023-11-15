using System;
using ChartAndGraph;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestMonthWeek : MonoBehaviour
{
    public GraphChart chart;

    // Start is called before the first frame update
    private void Start()
    {
        chart.DataSource.ClearCategory("Player 1");
        var now = DateTime.Now;
        for (var i = 0; i < 60; i++)
            chart.DataSource.AddPointToCategory("Player 1", now.AddMonths(i), Random.Range(0f, 10f));
    }

    // Update is called once per frame
    private void Update()
    {
    }
}