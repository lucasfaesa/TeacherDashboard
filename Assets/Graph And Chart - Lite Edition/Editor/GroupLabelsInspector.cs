using UnityEditor;

namespace ChartAndGraph
{
    [CustomEditor(typeof(GroupLabels))]
    internal class GroupLabelsInspector : ItemLabelsBaseEditor
    {
        protected override string Name => "group labels";

        protected override bool isSupported(AnyChart chart)
        {
            return ((IInternalUse)chart).InternalSupportsGroupLabels;
        }
    }
}