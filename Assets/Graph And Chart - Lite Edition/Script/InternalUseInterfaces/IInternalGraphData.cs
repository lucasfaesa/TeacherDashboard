using System;
using System.Collections.Generic;

namespace ChartAndGraph
{
    internal interface IInternalGraphData
    {
        int TotalCategories { get; }
        IEnumerable<GraphData.CategoryData> Categories { get; }

        /// <summary>
        /// </summary>
        /// <param name="axis">0 for horizontal 1 for vertical</param>
        /// <returns></returns>
        double GetMinValue(int axis, bool dataValue);

        double GetMaxValue(int axis, bool dataValue);
        void OnBeforeSerialize();
        void OnAfterDeserialize();
        void Update();
        event EventHandler InternalDataChanged;
        event EventHandler InternalViewPortionChanged;
        event Action<int, string> InternalRealTimeDataChanged;
    }
}