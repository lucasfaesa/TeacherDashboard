namespace ChartAndGraph
{
    public interface IBarGenerator
    {
        void Generate(float normalizedSize, float scale);
        void Clear();
    }
}