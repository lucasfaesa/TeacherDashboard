using UnityEditor;

namespace ChartAndGraph
{
    internal abstract class ItemLabelsBaseEditor : Editor
    {
        protected abstract string Name { get; }
        protected abstract bool isSupported(AnyChart chart);

        public override void OnInspectorGUI()
        {
            var labels = (ItemLabelsBase)target;

            if (labels.gameObject == null)
                return;

            var chart = labels.gameObject.GetComponent<AnyChart>();
            if (chart == null)
                return;
            if (isSupported(chart) == false)
            {
                EditorGUILayout.HelpBox(
                    string.Format("Chart of type {0} does not support {1}", chart.GetType().Name, Name),
                    MessageType.Warning);
                return;
            }

            base.OnInspectorGUI();
        }
    }
}