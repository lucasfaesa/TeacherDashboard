using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChartDataEditor : EditorWindow
{
    private SerializedProperty mBarData;
    private SerializedProperty mCategories;
    private SerializedProperty mData;
    private SerializedObject mEditedObject;
    private SerializedProperty mGroups;
    private Dictionary<string, SerializedProperty> mValues;

    private void OnGUI()
    {
        GUILayout.Label("Edit Values", EditorStyles.boldLabel);
        GUILayout.BeginVertical();
        var categoryCount = mCategories.arraySize;
        var groupCount = mGroups.arraySize;
        GUILayout.BeginHorizontal();

        GUILayout.Label(" ", GUILayout.Width(EditorGUIUtility.fieldWidth));
        for (var i = 0; i < groupCount; i++)
        {
            var group = mGroups.GetArrayElementAtIndex(i).stringValue;
            GUILayout.Label(group, GUILayout.Width(EditorGUIUtility.fieldWidth));
        }

        GUILayout.EndHorizontal();
        for (var i = 0; i < categoryCount; i++)
        {
            var catProp = mCategories.GetArrayElementAtIndex(i);
            var category = catProp.FindPropertyRelative("Name").stringValue;
            GUILayout.BeginHorizontal();
            GUILayout.Label(category, GUILayout.Width(EditorGUIUtility.fieldWidth));
            for (var j = 0; j < groupCount; j++)
            {
                var group = mGroups.GetArrayElementAtIndex(j).stringValue;
                var keyName = getKey(category, group);
                var value = 0.0;
                SerializedProperty prop;
                if (mValues.TryGetValue(keyName, out prop))
                    value = prop.doubleValue;
                else
                    prop = null;
                var newVal = EditorGUILayout.DoubleField(value, GUILayout.Width(EditorGUIUtility.fieldWidth));
                if (newVal != value)
                    prop.doubleValue = newVal;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        mEditedObject.ApplyModifiedProperties();
    }

    public static ChartDataEditor ShowForObject(SerializedObject obj)
    {
        var window = (ChartDataEditor)GetWindow(typeof(ChartDataEditor));
        window.SetEditedObject(obj);
        return window;
    }

    public void SetEditedObject(SerializedObject obj)
    {
        mEditedObject = obj;
        mBarData = mEditedObject.FindProperty("Data");
        mCategories = mBarData.FindPropertyRelative("mCategories");
        mGroups = mBarData.FindPropertyRelative("mGroups");
        mData = mBarData.FindPropertyRelative("mData");
        LoadData();
    }

    private void LoadData()
    {
        if (mValues == null)
            mValues = new Dictionary<string, SerializedProperty>();
        mValues.Clear();
        var size = mData.arraySize;
        for (var i = 0; i < size; i++)
        {
            var prop = mData.GetArrayElementAtIndex(i);
            var columnName = prop.FindPropertyRelative("ColumnName").stringValue;
            var rowName = prop.FindPropertyRelative("GroupName").stringValue;
            var amount = prop.FindPropertyRelative("Amount");
            var keyName = getKey(columnName, rowName);
            mValues[keyName] = amount;
        }
    }

    private string getKey(string column, string row)
    {
        return string.Format("{0}|{1}", column, row);
    }
}