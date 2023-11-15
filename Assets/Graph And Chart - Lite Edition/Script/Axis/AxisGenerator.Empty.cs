using UnityEngine;

namespace ChartAndGraph.Axis
{
    public partial class AxisGenerator : MonoBehaviour, IAxisGenerator
    {
        public void FixLabels(AnyChart parent)
        {
            InnerFixLabels(parent);
        }

        public void SetAxis(double scrollOffset, AnyChart parent, AxisBase axis, ChartOrientation axisOrientation,
            int divType)
        {
            InnerSetAxis(scrollOffset, parent, axis, axisOrientation, divType);
        }

        public Object This()
        {
            return this;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        partial void InnerFixLabels(AnyChart parent);

        partial void InnerSetAxis(double scrollOffset, AnyChart parent, AxisBase axis, ChartOrientation axisOrientation,
            int divType);
    }
}