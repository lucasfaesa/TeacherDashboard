using System;

namespace ChartAndGraph.DataSource
{
    /// <summary>
    ///     base class for data items (rows and columns) in a data source
    /// </summary>
    internal abstract class ChartDataItemBase : IDataItem
    {
        private string mName;
        private string mPrevName;

        public ChartDataItemBase(string name)
        {
            mName = name;
        }

        public object UserData { get; set; }

        /// <summary>
        ///     The material for this data source item
        /// </summary>
        public ChartDynamicMaterial Material { get; set; }

        public string Name
        {
            get => mName;
            set
            {
                mPrevName = mName;
                mName = value;
                if (NameChanged != null)
                    NameChanged(mPrevName, this);
            }
        }

        public event Action<string, IDataItem> NameChanged;

        public void CancelNameChange()
        {
            mName = mPrevName;
        }
    }
}