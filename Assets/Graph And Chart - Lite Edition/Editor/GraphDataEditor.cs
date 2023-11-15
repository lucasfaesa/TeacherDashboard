using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal class GraphDataEditor : EditorWindow
{
    [SerializeField] private Vector2[] Data;

    private string category;
    private readonly Func<Vector2[]> FillDataCustomCode = null;
    private SerializedProperty mCategories;
    private SerializedProperty mCategory;
    private SerializedObject mEditedObject;
    private SerializedProperty mGraphData;
    private SerializedObject mObject;
    private SerializedObject mThisObject;
    private Dictionary<string, SerializedProperty> mValues;

    private void OnGUI()
    {
        var serialProp = mThisObject.FindProperty("Data");

        GUILayout.Label("Edit Values - " + category, EditorStyles.boldLabel);

        if (mCategory == null)
            return;
        Vector2[] customArr = null;
        //FillDataCustomCode = FillDataCustomCodeImplementation;
        if (FillDataCustomCode != null)
            if (GUILayout.Button("Fill Data From Custom Code"))
                customArr = FillDataCustomCode();
        EditorGUILayout.PropertyField(serialProp, true);

        var arr = mCategory.FindPropertyRelative("InitialData");
        if (customArr != null)
        {
            SetArray(customArr, arr);
        }
        else
        {
            if (mThisObject.ApplyModifiedProperties())
                SetArray(serialProp, arr);
        }

        mEditedObject.ApplyModifiedProperties();
    }

    public static GraphDataEditor ShowForObject(SerializedObject obj, string category)
    {
        var window = (GraphDataEditor)GetWindow(typeof(GraphDataEditor));
        window.SetEditedObject(obj, category);
        return window;
    }

    private int FindCategoryIndex(string category)
    {
        for (var i = 0; i < mCategories.arraySize; i++)
        {
            var name = mCategories.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
            if (name == category)
                return i;
        }

        return -1;
    }

    public void SetEditedObject(SerializedObject obj, string categoryName)
    {
        category = categoryName;
        mEditedObject = obj;

        mGraphData = mEditedObject.FindProperty("Data");
        mCategories = mGraphData.FindPropertyRelative("mSerializedData");
        //   LoadData();

        var catIndex = FindCategoryIndex(categoryName);
        if (catIndex == -1)
        {
            mCategory = null;
            return;
        }

        mCategory = mCategories.GetArrayElementAtIndex(catIndex);

        var arr = mCategory.FindPropertyRelative("InitialData");

        mThisObject = new SerializedObject(this);
        var serialProp = mThisObject.FindProperty("Data");
        SetArray(arr, serialProp);
    }

    private string getKey(string column, string row)
    {
        return string.Format("{0}|{1}", column, row);
    }

    private void ShowCategoryArray()
    {
    }

    private void SetArray(SerializedProperty from, SerializedProperty to)
    {
        to.arraySize = from.arraySize;
        for (var i = 0; i < from.arraySize; i++)
        {
            var val = from.GetArrayElementAtIndex(i).vector2Value;
            to.GetArrayElementAtIndex(i).vector2Value = val;
        }
    }

    private void SetArray(Vector2[] from, SerializedProperty to)
    {
        to.arraySize = from.Length;
        for (var i = 0; i < from.Length; i++)
        {
            var val = from[i];
            to.GetArrayElementAtIndex(i).vector2Value = val;
        }
    }

    private Vector2[] FillDataCustomCodeImplementation()
    {
        return new[] { new(1, 1), new Vector2(2, 2), new Vector2(3, 3), new Vector2(4, 4) };
    }
}