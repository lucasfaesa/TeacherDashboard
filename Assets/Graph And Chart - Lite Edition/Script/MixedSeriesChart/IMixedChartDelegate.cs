using System;
using System.Collections.Generic;

namespace ChartAndGraph
{
    internal interface IMixedChartDelegate
    {
        ScrollableAxisChart CreateCategoryView(Type t, ScrollableAxisChart prefab);
        void SetData(Dictionary<string, BaseScrollableCategoryData> data);
        void RealaseChart(ScrollableAxisChart chart);
        void DeactivateChart(ScrollableAxisChart chart);
        void ReactivateChart(ScrollableAxisChart chart);
    }
}