using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

public class MarkerText : MonoBehaviour
{
    public GraphChartBase Chart;
    public Text TextObject;

    /// <summary>
    ///     the text position in graph coordinates
    /// </summary>
    public DoubleVector2 point;

    /// <summary>
    ///     offsets the text on the x axis away from the selected point
    /// </summary>
    public float SeperationOffset;

    /// <summary>
    ///     offsets the text on the y axus away from the selected point
    /// </summary>
    public float ElevationOffset;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Chart == null)
            return;
        Vector3 res;

        if (Chart.PointToWorldSpace(out res, point.x, point.y))
            TextObject.transform.position = res + new Vector3(SeperationOffset, ElevationOffset, 0f);
    }
}