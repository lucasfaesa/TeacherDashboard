using ChartAndGraph;
using UnityEngine;

public class RadarMaxValueCategory : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        var radar = GetComponent<RadarChart>();
        if (radar != null)
        {
            radar.DataSource.SetCategoryMaxValue("Player 1", 20);
            radar.DataSource.SetValue("Player 1", "A", 10);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}