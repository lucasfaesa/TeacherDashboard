using ChartAndGraph;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ChartOrientedSize))]
internal class ChartOrientedSizeInspector : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2;
    }

    private void DoField(SerializedProperty prop, string label, Rect position)
    {
        var size = GUI.skin.label.CalcSize(new GUIContent(label)).x;
        var labelRect = new Rect(position.x, position.y, size, position.height);
        var FieldRect = new Rect(labelRect.xMax, position.y, position.width - size, position.height);
        EditorGUI.LabelField(labelRect, label);
        var val = prop.floatValue;
        EditorGUI.LabelField(labelRect, label);
        var labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 5;
        val = EditorGUI.FloatField(FieldRect, " ", val);
        EditorGUIUtility.labelWidth = labelWidth;
        prop.floatValue = val;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        position = EditorGUI.IndentedRect(position);

        var halfWidth = position.width * 0.5f;
        var y = position.y + EditorGUIUtility.singleLineHeight;
        var height = position.height - EditorGUIUtility.singleLineHeight;
        var breadthRect = new Rect(position.x, y, halfWidth, height);
        var depthRect = new Rect(breadthRect.xMax, y, halfWidth, height);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        var breadth = property.FindPropertyRelative("Breadth");
        var depth = property.FindPropertyRelative("Depth");
        DoField(breadth, "Breadth:", breadthRect);
        DoField(depth, "Depth:", depthRect);
        EditorGUI.indentLevel = indent;
        // EditorGUILayout.EndVertical();
        EditorGUI.EndProperty();
    }
}