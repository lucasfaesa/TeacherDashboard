using UnityEditor;

namespace ChartAndGraph
{
    [CustomEditor(typeof(CategoryLabels))]
    internal class CategoryLabelsLabelsInspector : ItemLabelsBaseEditor
    {
        protected override string Name => "category labels";

        protected override bool isSupported(AnyChart chart)
        {
            return ((IInternalUse)chart).InternalSupportsCategoryLables;
        }
    }
}