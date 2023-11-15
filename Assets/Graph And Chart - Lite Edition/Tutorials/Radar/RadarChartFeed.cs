using ChartAndGraph;
using UnityEngine;

public class RadarChartFeed : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        var radar = GetComponent<RadarChart>();
        if (radar != null) radar.DataSource.SetValue("Player 1", "A", 10);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}