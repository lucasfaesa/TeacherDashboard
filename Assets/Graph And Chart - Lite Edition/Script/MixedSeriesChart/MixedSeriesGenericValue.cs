namespace ChartAndGraph
{
    /// <summary>
    ///     represets a generic datum in a chart. This datum can be a point in a graph , a candle stick, a bar or anything else
    /// </summary>
    public struct MixedSeriesGenericValue
    {
        /// <summary>
        ///     the name of the object. Used in item lables
        /// </summary>
        private string name;

        /// <summary>
        ///     the index of the item. This can be used for example in stacked bar chart. Objects with the same item value are the
        ///     stacks of the same bar
        /// </summary>
        private int index;

        /// <summary>
        ///     the index of this item within a parent item. for example bar stacks of the same bar have different subIndex value
        /// </summary>
        private int subIndex;

        /// <summary>
        ///     defines points that can be used in different ways by different series
        /// </summary>
        private double x, y;

        /// <summary>
        ///     defines points that can be used in different ways by different series
        /// </summary>
        private double x1, y1;

        /// <summary>
        ///     defines a size that can be used in different ways by different series
        /// </summary>
        private double size;

        /// <summary>
        ///     defines the high end of a custom range that can used in diffrent ways by different series
        /// </summary>
        private double high;

        /// <summary>
        ///     defines the low end of a custom range that can be used in different ways by different series
        /// </summary>
        private double low;

        /// <summary>
        ///     this can contain just about anything. This value is passed to child prefabs, use this to create custom behaviors
        ///     such as a chart within a chart etc
        /// </summary>
        private object userData;
    }
}