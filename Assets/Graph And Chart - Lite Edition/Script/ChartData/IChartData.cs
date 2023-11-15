namespace ChartAndGraph
{
    public interface IChartData
    {
        void Update();
        void OnAfterDeserialize();
        void OnBeforeSerialize();
    }
}