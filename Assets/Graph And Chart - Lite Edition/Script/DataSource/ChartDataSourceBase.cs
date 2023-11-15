using System;
using ChartAndGraph.Common;

namespace ChartAndGraph.DataSource
{
    /// <summary>
    ///     base class for all data sources
    /// </summary>
    internal abstract class ChartDataSourceBase
    {
        public abstract ChartColumnCollection Columns { get; }
        public abstract ChartRowCollection Rows { get; }

        public event EventHandler DataStructureChanged;
        public event Action<string, int, string, int> ItemsReplaced;
        public event EventHandler<DataValueChangedEventArgs> DataValueChanged;

        protected void OnDataStructureChanged()
        {
            if (DataStructureChanged != null)
                DataStructureChanged(this, EventArgs.Empty);
        }

        protected void OnItemsReplaced(string first, int firstIndex, string second, int secondIndex)
        {
            if (ItemsReplaced != null)
                ItemsReplaced(first, firstIndex, second, secondIndex);
        }

        protected void OnDataValueChanged(DataValueChangedEventArgs data)
        {
            if (DataValueChanged != null)
                DataValueChanged(this, data);
        }

        public abstract double[,] getRawData();

        public class DataValueChangedEventArgs : EventArgs
        {
            public DataValueChangedEventArgs(int group, int category, double oldValue, double newValue,
                bool minMaxChanged)
            {
                ItemIndex = new ChartItemIndex(group, category);
                OldValue = oldValue;
                NewValue = newValue;
                MinMaxChanged = minMaxChanged;
            }

            public ChartItemIndex ItemIndex { get; }
            public double OldValue { get; }
            public double NewValue { get; }
            public bool MinMaxChanged { get; }
        }
    }
}