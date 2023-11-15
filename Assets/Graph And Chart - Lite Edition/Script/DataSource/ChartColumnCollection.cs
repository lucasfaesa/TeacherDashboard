namespace ChartAndGraph.DataSource
{
    /// <summary>
    ///     Collection of columns on a data source
    /// </summary>
    internal class ChartColumnCollection : ChartDataSourceBaseCollection<ChartDataColumn>
    {
        protected override string ItemTypeName => "category";
    }
}