using ChartAndGraph;
using UnityEngine;

public class PyramidAnimation : MonoBehaviour
{
    public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public bool AnimateOnStart = true;
    public float AnimationTime = 3f;
    private float animationStart = -1f;

    private PyramidChart chart;

    // Use this for initialization
    private void Start()
    {
        chart = GetComponent<PyramidChart>();
        if (AnimateOnStart)
            Animate();
    }

    // Update is called once per frame
    private void Update()
    {
        if (chart == null)
            return;
        if (animationStart < 0f)
            return;
        var elasped = (Time.time - animationStart) / AnimationTime;
        elasped = Mathf.Clamp(elasped, 0f, 1f);
        var blend = Curve.Evaluate(elasped);
        for (var i = 0; i < chart.DataSource.TotalCategories; i++)
        {
            var name = chart.DataSource.GetCategoryName(i);
            chart.DataSource.SetCategoryAlpha(name, blend);
            chart.DataSource.SetCategoryOrientation(name, blend, 0f, 0f);
        }
    }

    public void Animate()
    {
        animationStart = Time.time;
    }
}