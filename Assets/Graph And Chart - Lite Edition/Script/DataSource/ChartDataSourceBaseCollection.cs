using System;
using System.Collections;
using System.Collections.Generic;
using ChartAndGraph.DataSource;
using ChartAndGraph.Exceptions;

namespace ChartAndGraph
{
    /// <summary>
    ///     base class for all data source collections. (row and column collections)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ChartDataSourceBaseCollection<T> : ICollection<T> where T : class, IDataItem
    {
        private readonly List<T> mItems = new();
        private readonly Dictionary<string, T> mNameToItem = new();

        /// <summary>
        ///     Used for messages and exceptions. this can be "column" if the derived class represents a column collestion
        /// </summary>
        protected abstract string ItemTypeName { get; }

        public T this[int index] => mItems[index];

        public T this[string name]
        {
            get
            {
                T res;
                if (mNameToItem.TryGetValue(name, out res) == false)
                    throw new ChartItemNotExistException(
                        string.Format("A {0} by the name {1} does not exist in the collection", ItemTypeName, name));
                return res;
            }
        }

        /// <summary>
        ///     Adds an item to the last index of the collection. throws and exception if an item by that name already exists
        /// </summary>
        /// <param name="item">the new item to add</param>
        public void Add(T item)
        {
            Insert(mItems.Count, item); // call insert
        }

        /// <summary>
        ///     clear the collection
        /// </summary>
        public void Clear()
        {
            foreach (var item in mItems)
            {
                item.NameChanged -= NameChangedHandler;
                if (ItemRemoved != null)
                    ItemRemoved(item);
            }

            mItems.Clear(); // call the corresponding list method
            mNameToItem.Clear(); // and clear the name to index dictionary
            if (OrderChanged != null)
                OrderChanged(this, EventArgs.Empty);
        }

        public bool Contains(T item)
        {
            return mItems.Contains(item); // call the corresponding list method
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            mItems.CopyTo(array, arrayIndex); // call the corresponding list method
        }

        public int Count => mItems.Count; // call the corresponding list method

        public bool IsReadOnly => false;

        public bool Remove(T item)
        {
            if (mItems.Remove(item)) // remove the item from the list
            {
                if (item.Name != null) // remove name to index
                    mNameToItem.Remove(item.Name);
                item.NameChanged -= NameChangedHandler; // remove event handler for name change
                if (ItemRemoved != null)
                    ItemRemoved(item);
                if (OrderChanged != null)
                    OrderChanged(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return mItems.GetEnumerator(); // call the corresponding list method
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mItems.GetEnumerator(); // call the corresponding list method
        }

        public void SwitchPositions(string first, string second)
        {
            int firstIndex, secondIndex;
            if (TryGetIndexByName(first, out firstIndex) == false)
                throw new ChartItemNotExistException(
                    string.Format("A {0} by the name {1} does not exist in the collection", ItemTypeName, first));
            if (TryGetIndexByName(second, out secondIndex) == false)
                throw new ChartItemNotExistException(
                    string.Format("A {0} by the name {1} does not exist in the collection", ItemTypeName, second));
            SwitchPositions(firstIndex, secondIndex);
        }

        public void SwitchPositions(int first, int second)
        {
            var tmp = mItems[first];
            mItems[first] = mItems[second];
            mItems[second] = tmp;
            if (ItemsReplaced != null)
                ItemsReplaced(mItems[first].Name, first, mItems[second].Name, second);
        }

        public void Move(string name, int newPosition)
        {
            if (newPosition >= Count || newPosition < 0)
                throw new IndexOutOfRangeException();
            int currentPosition;
            if (TryGetIndexByName(name, out currentPosition) == false)
                throw new ChartItemNotExistException(
                    string.Format("A {0} by the name {1} does not exist in the collection", ItemTypeName, name));
            var advance = Math.Sign(newPosition - currentPosition);
            if (advance == 0) // same position
                return;
            for (var i = currentPosition; i != newPosition; i += advance)
                SwitchPositions(i, i + advance);
        }

        public bool TryGetIndexByName(string name, out int result)
        {
            for (var i = 0; i < mItems.Count; ++i)
                if (mItems[i].Name == name)
                {
                    result = i;
                    return true;
                }

            result = -1;
            return false;
        }

        /// <summary>
        ///     inserts and item at the specified index of the collection. throws and exception if an item by that name already
        ///     exists
        /// </summary>
        /// <param name="item">the new item to add</param>
        public void Insert(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException("item argument can't be null");
            if (item.Name != null)
                if (mNameToItem.ContainsKey(item.Name))
                    throw new ChartDuplicateItemException(
                        string.Format("A {0} by that name already exists in the data source", ItemTypeName));
            // check if it already contained in the collection
            if (mItems.Contains(item))
                throw new ChartDuplicateItemException(string.Format("The {0} is already added to the collection",
                    ItemTypeName));
            // insert the item
            mItems.Insert(index, item);
            item.NameChanged += NameChangedHandler;
            if (item.Name != null) // make sure the name to index points to it
                mNameToItem.Add(item.Name, item);
            if (OrderChanged != null)
                OrderChanged(this, EventArgs.Empty);
        }

        private void NameChangedHandler(string prevName, IDataItem item)
        {
            if (prevName == item.Name) // ignore identical names
                return;
            if (mNameToItem.ContainsKey(item.Name)) // check duplicate names
            {
                item.CancelNameChange();
                throw new ChartDuplicateItemException(
                    string.Format("A {0} by that name already exists in the data source", ItemTypeName));
            }

            if (prevName != null && mNameToItem.Remove(prevName) == false)
            {
                item.CancelNameChange();
                throw new ChartException(string.Format("Renaming {0} failed", ItemTypeName));
            }

            if (item.Name != null)
                mNameToItem.Add(item.Name, (T)item);
            if (NameChanged != null)
                NameChanged(prevName, item);
        }

#pragma warning disable 0067
        public event Action<string, IDataItem> NameChanged;
        public event EventHandler OrderChanged;
        public event Action<T> ItemRemoved;
        public event Action<string, int, string, int> ItemsReplaced;

#pragma warning restore 0067
    }
}