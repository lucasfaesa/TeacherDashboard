using UnityEngine;

namespace ChartAndGraph.Axis
{
    /// <summary>
    ///     Axis generator functionallity that is not dependant on the diminetion of the chart
    /// </summary>
    public interface IAxisGenerator
    {
        Object This();
        GameObject GetGameObject();
        void FixLabels(AnyChart parent);

        void SetAxis(double scrollOffset, AnyChart parent, AxisBase axis, ChartOrientation axisOrientation,
            int divType);
    }
}