using ChartAndGraph;
using UnityEngine;

public class PieChartFeed : MonoBehaviour
{
    private void Start()
    {
        var pie = GetComponent<PieChart>();
        if (pie != null)
        {
            pie.DataSource.SlideValue("Player 1", 50, 10f);
            pie.DataSource.SetValue("Player 2", Random.value * 10);
        }
    }
}