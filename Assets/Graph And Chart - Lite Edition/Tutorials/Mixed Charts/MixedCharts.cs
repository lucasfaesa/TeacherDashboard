using System.Collections;
using ChartAndGraph;
using UnityEngine;

public class MixedCharts : MonoBehaviour
{
    public BarChart Bar;

    public GraphChartBase Graph;

    // Use this for initialization
    private void Start()
    {
        StartCoroutine(FillGraphWait());
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /// <summary>
    ///     This method waits for the bar chart to fill up before filling the graph
    /// </summary>
    /// <returns></returns>
    private IEnumerator FillGraphWait()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        FillGraph();
    }

    private void FillGraph()
    {
        Graph.DataSource.ClearCategory("Category1");
        for (var i = 0; i < Bar.DataSource.TotalCategories; i++)
        {
            var categoryName = Bar.DataSource.GetCategoryName(i);
            for (var j = 0; j < Bar.DataSource.TotalGroups; j++)
            {
                var groupName = Bar.DataSource.GetGroupName(j);
                Vector3 position;
                Bar.GetBarTrackPosition(categoryName, groupName,
                    out position); // find the position of the top of the bar chart
                double x, y;
                Graph.PointToClient(position, out x, out y); // convert it to graph coordinates
                Graph.DataSource.AddPointToCategory("Category1", x,
                    Random.value * 10f); // drop the y value and set your own value
            }
        }
    }
}