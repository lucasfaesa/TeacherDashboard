using System;

namespace ChartAndGraph
{
    internal interface IInternalRadarData
    {
        ChartSparseDataSource InternalDataSource { get; }
        double GetMinValue();
        double GetMaxValue();
        event EventHandler InternalDataChanged;
        RadarChartData.CategoryData getCategoryData(int i);
    }
}