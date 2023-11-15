namespace ChartAndGraph
{
    internal interface IInternalBarData
    {
        ChartSparseDataSource InternalDataSource { get; }
        void Update();
        double GetMinValue();
        double GetMaxValue();
        void OnBeforeSerialize();
        void OnAfterDeserialize();
    }
}