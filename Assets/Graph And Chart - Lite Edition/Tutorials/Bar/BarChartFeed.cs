﻿using ChartAndGraph;
using UnityEngine;

public class BarChartFeed : MonoBehaviour
{
    private void Start()
    {
        var barChart = GetComponent<BarChart>();
        if (barChart != null)
        {
            barChart.DataSource.SetValue("Player 1", "Value 1", Random.value * 20);
            barChart.DataSource.SlideValue("Player 2", "Value 1", Random.value * 20, 40f);
        }
    }

    private void Update()
    {
    }
}