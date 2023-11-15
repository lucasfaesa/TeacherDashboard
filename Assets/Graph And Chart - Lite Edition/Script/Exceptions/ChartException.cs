using System;

namespace ChartAndGraph.Exceptions
{
    internal class ChartException : Exception
    {
        public ChartException(string message)
            : base(message)
        {
        }
    }
}