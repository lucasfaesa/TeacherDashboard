using System;
using ChartAndGraph;
using UnityEngine;
using Random = UnityEngine.Random;

public class customAxisDivisions : MonoBehaviour
{
    public GraphChart chart;

    // Start is called before the first frame update
    private void Start()
    {
        chart.DataSource.ClearCategory("Player 1");
        var now = DateTime.Now;
        for (var i = 0; i < 36; i++)
            chart.DataSource.AddPointToCategory("Player 1", now + TimeSpan.FromDays(i * 10), Random.Range(0f, 10f));
        var month = now;
        chart.ClearHorizontalCustomDivisions();
        for (var i = 0; i < 12; i++)
        {
            chart.AddHorizontalAxisDivision(ChartDateUtility.DateToValue(month));
            month = month.AddMonths(1);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}