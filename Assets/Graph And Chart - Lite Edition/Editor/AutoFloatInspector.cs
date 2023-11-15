using UnityEditor;
using UnityEngine;

namespace ChartAndGraph
{
    [CustomPropertyDrawer(typeof(AutoFloat))]
    internal class AutoFloatInspector : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var auto = property.FindPropertyRelative("Automatic");
            var val = property.FindPropertyRelative("Value");
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var res = EditorGUI.ToggleLeft(position, "Auto", auto.boolValue);
            EditorGUI.indentLevel = indent;
            EditorGUI.indentLevel++;
            if (auto.boolValue == false && EditorGUI.showMixedValue == false)
                val.floatValue = EditorGUILayout.FloatField("Value", val.floatValue);
            auto.boolValue = res;
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}