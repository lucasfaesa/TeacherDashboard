﻿namespace ChartAndGraph.Common
{
    /// <summary>
    ///     and index of a chart item. this include group and category data.
    /// </summary>
    internal struct ChartItemIndex
    {
        public ChartItemIndex(int group, int category) : this()
        {
            Group = group;
            Category = category;
        }

        public override bool Equals(object obj)
        {
            if (obj is ChartItemIndex)
            {
                var index = (ChartItemIndex)obj;
                if (index.Group == Group && index.Category == Category)
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Group.GetHashCode() ^ Category.GetHashCode();
        }

        public int Group { get; set; }
        public int Category { get; set; }
    }
}