using System;

namespace ChartAndGraph.DataSource
{
    /// <summary>
    ///     base interface for data items (columns and charts)
    /// </summary>
    public interface IDataItem
    {
        string Name { get; set; }
        void CancelNameChange();

        /// <summary>
        ///     the first string argument contains the preivous name used by the item
        ///     the second object argumnet is the item itself
        /// </summary>
        event Action<string, IDataItem> NameChanged;
    }
}