namespace ChartAndGraph.Exceptions
{
    internal class ChartDuplicateItemException : ChartException
    {
        public ChartDuplicateItemException(string message)
            : base(message)
        {
        }
    }
}