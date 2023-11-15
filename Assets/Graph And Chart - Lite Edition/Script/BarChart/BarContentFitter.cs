using ChartAndGraph;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasBarChart))]
public class BarContentFitter : MonoBehaviour
{
    public float RatioAxisMarign = 1f;
    public float FixedBarSize = 30f;
    public float RatioGroupSeperation = 5f;

    public float RatioBarSeperation = 2f;

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
        var columnCount = bar.DataSource.TotalCategories;
        var rowCount = bar.DataSource.TotalGroups;

        var rowLimit = rowCount - 1;
        double barGroupSeprationSize = RatioBarSeperation * (columnCount - 1);
        var barGroupSize = barGroupSeprationSize; // + RatioBarSize;
        double totalSize = RatioGroupSeperation * rowLimit;
        var baseSize = totalSize + 2 * RatioAxisMarign + barGroupSize;

        var factor = totalWidth / baseSize;

        bar.HeightRatio = rect.rect.height;
        bar.AxisSeperation = (float)(RatioAxisMarign * factor);
        bar.BarSeperation = (float)(RatioBarSeperation * factor);
        bar.GroupSeperation = (float)(RatioGroupSeperation * factor);
        bar.BarSize = FixedBarSize; // (float)(RatioBarSize * factor);
    }
}