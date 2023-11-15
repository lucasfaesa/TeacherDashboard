using ChartAndGraph;
using UnityEngine;

public class CustomFormat : MonoBehaviour
{
    public GraphChart chart;

    // Start is called before the first frame update
    private void Start()
    {
        chart.CustomNumberFormat = (nubmer, fractionDigits) => { return (int)(nubmer / 1000) + "K"; };
        chart.CustomDateTimeFormat = date => { return date.ToString("MMM"); };
    }

    // Update is called once per frame
    private void Update()
    {
    }
}