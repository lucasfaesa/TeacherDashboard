using ChartAndGraph;
using UnityEngine;

public class BarAnimation : MonoBehaviour
{
    public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public bool AnimateOnStart = true;

    // public bool AnimateOnEnable = true;
    public float AnimationTime = 3f;
    private BarChart barChart;

    // Use this for initialization
    private void Start()
    {
        barChart = GetComponent<BarChart>();
        if (AnimateOnStart)
            Animate();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /*   public void OnEnable()
       {
           barChart = GetComponent<BarChart>();
           if (AnimateOnEnable)
               Animate();
       }*/
    public void Animate()
    {
        if (barChart != null)
        {
            var max = barChart.DataSource.GetMaxValue();
            var min = barChart.DataSource.GetMinValue();
            barChart.DataSource.StartBatch();
            barChart.DataSource.AutomaticMaxValue = false;
            barChart.DataSource.AutomaticMinValue = false;
            barChart.DataSource.MaxValue = max;
            barChart.DataSource.MinValue = min;
            for (var i = 0; i < barChart.DataSource.TotalCategories; i++)
            for (var j = 0; j < barChart.DataSource.TotalGroups; j++)
            {
                var category = barChart.DataSource.GetCategoryName(i);
                var group = barChart.DataSource.GetGroupName(j);
                var val = barChart.DataSource.GetValue(category, group);
                barChart.DataSource.SetValue(category, group, 0.0);
                barChart.DataSource.SlideValue(category, group, val, AnimationTime, Curve);
            }

            barChart.DataSource.EndBatch();
        }
    }
}