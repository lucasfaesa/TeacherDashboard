using ChartAndGraph;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasBarChart))]
public class BarContentFiller : MonoBehaviour
{
    public float FixedAxisMarign = 40f;
    public float FixedGroupSpacing = 25f;

    public float FixedBarSeperation = 10f;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnValidate()
    {
        var bar = GetComponent<BarChart>();
        bar.Invalidate();
    }

    public virtual void Match()
    {
        var rect = GetComponent<RectTransform>();
        var totalWidth = rect.rect.width;
        var bar = GetComponent<CanvasBarChart>();
        var Stacked = bar.ViewType == BarChart.BarType.Stacked;
        var columnCount = bar.DataSource.TotalCategories;
        var rowCount = bar.DataSource.TotalGroups;

        var rowLimit = rowCount - 1;
        double groupSize = (totalWidth - FixedAxisMarign * 2 - FixedGroupSpacing * rowLimit) / rowCount;
        var groupSeperation = groupSize + FixedGroupSpacing;
        var barSize = (groupSize - FixedBarSeperation * (columnCount - 1)) / columnCount;
        if (Stacked)
            barSize = groupSize;

        bar.AxisSeperation = FixedAxisMarign;
        if (Stacked)
            bar.BarSeperation = 0;
        else
            bar.BarSeperation = FixedBarSeperation + (float)barSize;

        bar.GroupSeperation = (float)groupSeperation;
        bar.BarSize = (float)barSize; // (float)(RatioBarSize * factor);
    }
}