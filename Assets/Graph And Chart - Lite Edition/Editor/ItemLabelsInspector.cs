using UnityEditor;

namespace ChartAndGraph
{
    [CustomEditor(typeof(ItemLabels))]
    internal class ItemLabelsLabelsInspector : ItemLabelsBaseEditor
    {
        protected override string Name => "item labels";

        protected override bool isSupported(AnyChart chart)
        {
            return ((IInternalUse)chart).InternalSupportsItemLabels;
        }
    }
}