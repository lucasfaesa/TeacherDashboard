using UnityEngine;

namespace ChartAndGraph
{
    public class PieInfo : MonoBehaviour
    {
        public PieChart.PieObject pieObject { get; set; }
        public string Category => pieObject.category;
    }
}