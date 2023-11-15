namespace ChartAndGraph.DataSource
{
    /// <summary>
    ///     row collection in a data source
    /// </summary>
    internal class ChartRowCollection : ChartDataSourceBaseCollection<ChartDataRow>
    {
        protected override string ItemTypeName => "group";
    }
}