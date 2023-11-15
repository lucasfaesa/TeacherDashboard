using System;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PyramidChart), true)]
internal class PyramidChartInspetor : Editor
{
    private readonly HashSet<string> mAllNames = new();
    private GUIStyle mBold;
    private bool mCategories;
    private string mCategoryError;
    private string mNewCategoryName = "";
    private readonly Dictionary<string, string> mOperations = new();
    private GUIStyle mRedStyle;

    private RenameWindow mRenameWindow;
    private Texture mSettings;
    private GUIStyle mSplitter;
    private readonly List<int> mToRemove = new();
    private readonly List<int> mToUp = new();
    private bool mUpdateWindow;
    private ChartDataEditor mWindow;

    public void OnEnable()
    {
        mRedStyle = new GUIStyle();
        mRedStyle.normal.textColor = Color.red;

        mSplitter = new GUIStyle();
        mSplitter.normal.background = EditorGUIUtility.whiteTexture;
        mSplitter.stretchWidth = true;
        mSplitter.margin = new RectOffset(0, 0, 7, 7);
    }

    private void OnDisable()
    {
        if (mRenameWindow != null)
        {
            mRenameWindow.Close();
            mRenameWindow = null;
        }

        if (mWindow != null)
        {
            mWindow.Close();
            mWindow = null;
        }
    }

    public void Splitter()
    {
        var position = GUILayoutUtility.GetRect(GUIContent.none, mSplitter, GUILayout.Height(1f));
        if (Event.current.type == EventType.Repaint)
        {
            var restoreColor = GUI.color;
            GUI.color = new Color(0.5f, 0.5f, 0.5f);
            mSplitter.Draw(position, false, false, false, false);
            GUI.color = restoreColor;
        }
    }

    private static bool IsAlphaNum(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        for (var i = 0; i < str.Length; i++)
            if (!char.IsLetter(str[i]) && !char.IsNumber(str[i]) && str[i] != ' ')
                return false;

        return true;
    }

    private void DoOperations(SerializedProperty items, int size, string type)
    {
        mToRemove.Clear();
        mToUp.Clear();
        var up = false;
        for (var i = 0; i < size; i++)
        {
            var entry = items.GetArrayElementAtIndex(i);
            if (entry == null)
                continue;
            var nameProp = entry.FindPropertyRelative("Name");
            string name = null;
            if (nameProp == null)
                name = entry.stringValue;
            else
                name = nameProp.stringValue;

            var arg = type + "|" + name;
            string res = null;
            if (up)
            {
                mToUp.Add(i);
                up = false;
            }

            if (mOperations.TryGetValue(arg, out res))
            {
                if (res == "remove")
                    mToRemove.Add(i);
                if (res == "up" && i > 0)
                    mToUp.Add(i);
                if (res == "down")
                    up = true;
                mOperations.Remove(arg);
            }
        }

        for (var i = 0; i < mToRemove.Count; i++)
            items.DeleteArrayElementAtIndex(mToRemove[i]);
        for (var i = 0; i < mToUp.Count; i++)
        {
            var cur = mToUp[i];
            items.MoveArrayElement(cur, cur - 1);
        }
    }

    private SerializedProperty getArrayCategory(SerializedProperty arr, string name)
    {
        for (var i = 0; i < arr.arraySize; i++)
        {
            var prop = arr.GetArrayElementAtIndex(i);
            if (prop.FindPropertyRelative("ColumnName").stringValue == name)
                return prop.FindPropertyRelative("Amount");
        }

        return null;
    }

    private void NamedItemEditor(SerializedProperty data, string type, string property, string caption,
        ref string errorMessage, ref bool foldout, ref string newName)
    {
        var items = data.FindPropertyRelative(property);
        var dataValues = data.FindPropertyRelative("mData");
        items.isExpanded = EditorGUILayout.Foldout(items.isExpanded, caption);
        //bool up, down;
        mAllNames.Clear();
        var size = items.arraySize;
        if (Event.current.type == EventType.Layout)
            DoOperations(items, size, type);
        size = items.arraySize;
        if (items.isExpanded)
        {
            EditorGUI.indentLevel++;
            for (var i = 0; i < size; i++)
            {
                var entry = items.GetArrayElementAtIndex(i);
                if (entry == null)
                    continue;
                var nameProp = entry.FindPropertyRelative("Name");
                string name = null;
                if (nameProp == null)
                    name = entry.stringValue;
                else
                    name = nameProp.stringValue;
                mAllNames.Add(name);

                var toogle = false;
                EditorGUILayout.BeginHorizontal();
                if (nameProp != null)
                {
                    toogle = entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, name);
                }
                else
                {
                    toogle = false;
                    EditorGUILayout.LabelField(name);
                }

                var valueProp = getArrayCategory(dataValues, name);
                GUILayout.FlexibleSpace();
                //    if(valueProp != null)
                //        EditorGUILayout.PropertyField(valueProp);
                if (GUILayout.Button("..."))
                    DoContext(type, name);
                EditorGUILayout.EndHorizontal();
                if (toogle)
                {
                    EditorGUI.indentLevel++;
                    if (nameProp != null)
                    {
                        var end = entry.GetEndProperty(true);
                        entry.Next(true);
                        if (SerializedProperty.EqualContents(entry, end) == false)
                            do
                            {
                                if (entry.name != "Name")
                                    EditorGUILayout.PropertyField(entry, true);
                                if (entry.name == "HeightRatio" && valueProp != null)
                                    valueProp.floatValue = entry.floatValue;
                            } while (entry.Next(entry.name == "Materials") &&
                                     SerializedProperty.EqualContents(entry, end) == false);
                    }

                    EditorGUI.indentLevel--;
                }
            }

            if (errorMessage != null)
                EditorGUILayout.LabelField(errorMessage, mRedStyle);
            EditorGUILayout.LabelField(string.Format("Add new {0} :", type));
            //Rect indentAdd = EditorGUI.IndentedRect(new Rect(0f, 0f, 1000f, 1000f));
            EditorGUILayout.BeginHorizontal();
            newName = EditorGUILayout.TextField(newName);
            //GUILayout.Space(indentAdd.xMin);
            if (GUILayout.Button("Add"))
            {
                var error = false;
                if (newName.Trim().Length == 0)
                {
                    errorMessage = "Name can't be empty";
                    error = true;
                }
                else if (IsAlphaNum(newName) == false)
                {
                    errorMessage = "Name conatins invalid characters";
                    error = true;
                }
                else if (mAllNames.Contains(newName))
                {
                    errorMessage = string.Format("A {0} named {1} already exists in this chart", type, newName);
                    error = true;
                }

                if (error == false)
                {
                    errorMessage = null;
                    items.InsertArrayElementAtIndex(size);
                    var newItem = items.GetArrayElementAtIndex(size);
                    var newItemName = newItem.FindPropertyRelative("Name");
                    if (newItemName == null)
                        newItem.stringValue = newName;
                    else
                        newItemName.stringValue = newName;
                    newName = "";
                    UpdateWindow();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        else
        {
            errorMessage = null;
        }

        UpdateWindow();
    }

    private void callback(object val)
    {
        var pair = (KeyValuePair<string, string>)val;
        mOperations[pair.Key] = pair.Value;
    }

    private bool RenameCategory(string fromName, string toName)
    {
        var pyramidChart = (PyramidChart)serializedObject.targetObject;
        try
        {
            pyramidChart.DataSource.RenameCategory(fromName, toName);
        }
        catch (Exception)
        {
            return false;
        }

        serializedObject.Update();
        if (pyramidChart.gameObject.activeInHierarchy)
            pyramidChart.GenerateChart();
        else
            EditorUtility.SetDirty(pyramidChart);
        return true;
    }

    private void RenameCalled(object val)
    {
        var data = (KeyValuePair<string, string>)val;
        var window = EditorWindow.GetWindow<RenameWindow>();
        mRenameWindow = window;
        if (data.Key == "category")
            window.ShowDialog(data.Value, data.Key, RenameCategory);
    }

    private void DoContext(string type, string name)
    {
        var arg = type + "|" + name;
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Move Up"), false, callback, new KeyValuePair<string, string>(arg, "up"));
        menu.AddItem(new GUIContent("Move Down"), false, callback, new KeyValuePair<string, string>(arg, "down"));
        menu.AddItem(new GUIContent("Remove"), false, callback, new KeyValuePair<string, string>(arg, "remove"));
        menu.AddItem(new GUIContent("Rename.."), false, RenameCalled, new KeyValuePair<string, string>(type, name));
        menu.ShowAsContext();
    }

    private void UpdateWindow()
    {
        mUpdateWindow = true;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var barData = serializedObject.FindProperty("Data");
        EditorGUILayout.BeginVertical();
        Splitter();
        if (mBold == null)
            mBold = new GUIStyle(EditorStyles.foldout);
        EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        NamedItemEditor(barData, "category", "mCategories", "Categories", ref mCategoryError, ref mCategories,
            ref mNewCategoryName);

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        if (mUpdateWindow)
        {
            mUpdateWindow = false;
            if (mWindow != null)
            {
                mWindow.SetEditedObject(serializedObject);
                mWindow.Repaint();
            }
        }
    }
}