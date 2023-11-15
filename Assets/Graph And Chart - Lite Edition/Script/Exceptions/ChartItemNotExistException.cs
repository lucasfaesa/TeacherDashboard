namespace ChartAndGraph.Exceptions
{
    internal class ChartItemNotExistException : ChartException
    {
        public ChartItemNotExistException(string message)
            : base(message)
        {
        }
    }
}