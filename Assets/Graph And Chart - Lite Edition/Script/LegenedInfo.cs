using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     Contains data about a chart legend
    /// </summary>
    public class LegenedData
    {
        /// <summary>
        ///     list of categories each with a name and material
        /// </summary>
        private readonly List<LegenedItem> mItems = new();

        /// <summary>
        ///     returns that data for this legend item
        /// </summary>
        public IEnumerable<LegenedItem> Items => mItems;

        public void AddLegenedItem(LegenedItem item)
        {
            mItems.Add(item);
        }

        /// <summary>
        ///     Each item has a name and material
        /// </summary>
        public class LegenedItem
        {
            public Material Material;
            public string Name;
        }
    }
}