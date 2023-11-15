namespace ChartAndGraph
{
    /// <summary>
    ///     base interface for pie mesh generators
    /// </summary>
    public interface IPieGenerator
    {
        void Generate(float startAngle, float angleSpan, float radius, float innerRadius, int segments,
            float outerDepth, float innerDepth);
    }
}