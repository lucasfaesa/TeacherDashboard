using System;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BarChart), true)]
internal class BarChartInspetor : Editor
{
    private readonly HashSet<string> mAllNames = new();

    private readonly GUIContent MaxBarValue = new("Max Bar Value :",
        "All the bars are scaled according to this value, Bars that are larger then this value are clamped");

    private GUIStyle mBold;
    private bool mCategories;
    private string mCategoryError;
    private string mGroupError;
    private bool mGroups;

    private readonly GUIContent MinBarValue = new("Min Bar Value :",
        "All the bars are scaled according to this value, Bars that are lower then this value are clamped");

    private string mNewCategoryName = "";
    private string mNewGroupName = "";
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

    private void NamedItemEditor(SerializedProperty data, string type, string property, string caption,
        ref string errorMessage, ref bool foldout, ref string newName)
    {
        var items = data.FindPropertyRelative(property);
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
                EditorGUILayout.BeginHorizontal();
                var toogle = false;
                if (nameProp != null)
                {
                    toogle = entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, name);
                }
                else
                {
                    toogle = false;
                    EditorGUILayout.LabelField(name);
                }

                GUILayout.FlexibleSpace();
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
                else if (ChartEditorCommon.IsAlphaNum(newName) == false)
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

    private bool RenameGroup(string fromName, string toName)
    {
        var barChart = (BarChart)serializedObject.targetObject;
        try
        {
            barChart.DataSource.RenameGroup(fromName, toName);
        }
        catch (Exception)
        {
            return false;
        }

        serializedObject.Update();
        if (barChart.gameObject.activeInHierarchy)
            barChart.GenerateChart();
        else
            EditorUtility.SetDirty(barChart);
        return true;
    }

    private bool RenameCategory(string fromName, string toName)
    {
        var barChart = (BarChart)serializedObject.targetObject;
        try
        {
            barChart.DataSource.RenameCategory(fromName, toName);
        }
        catch (Exception)
        {
            return false;
        }

        serializedObject.Update();
        if (barChart.gameObject.activeInHierarchy)
            barChart.GenerateChart();
        else
            EditorUtility.SetDirty(barChart);
        return true;
    }

    private void RenameCalled(object val)
    {
        var data = (KeyValuePair<string, string>)val;
        var window = EditorWindow.GetWindow<RenameWindow>();
        mRenameWindow = window;
        if (data.Key == "category")
            window.ShowDialog(data.Value, data.Key, RenameCategory);
        else if (data.Key == "group")
            window.ShowDialog(data.Value, data.Key, RenameGroup);
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
        serializedObject.Update();
        var barData = serializedObject.FindProperty("Data");
        EditorGUILayout.BeginVertical();
        Splitter();
        if (mBold == null) mBold = new GUIStyle(EditorStyles.foldout);
        //mBold.fontStyle = FontStyle.Bold;
        // EditorStyles.foldout.fontStyle = FontStyle.Bold;
        //   fold = EditorGUILayout.Foldout(fold,"Bar Data", EditorStyles.foldout);
        //EditorStyles.foldout.fontStyle = FontStyle.Normal;
        //  if (fold)
        // {
        EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        NamedItemEditor(barData, "category", "mCategories", "Categories", ref mCategoryError, ref mCategories,
            ref mNewCategoryName);
        NamedItemEditor(barData, "group", "mGroups", "Groups", ref mGroupError, ref mGroups, ref mNewGroupName);

        var maxProp = barData.FindPropertyRelative("maxValue");
        var minProp = barData.FindPropertyRelative("minValue");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(MinBarValue, EditorStyles.boldLabel);
        var automaticProp = barData.FindPropertyRelative("automaticMinValue");
        var automatic = automaticProp.boolValue;
        automatic = GUILayout.Toggle(automatic, "Auto");
        GUILayout.FlexibleSpace();
        automaticProp.boolValue = automatic;
        EditorGUILayout.EndHorizontal();
        if (automatic == false)
        {
            EditorGUILayout.PropertyField(minProp);
            if (minProp.doubleValue > maxProp.doubleValue)
                minProp.doubleValue = maxProp.doubleValue - 0.1;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(MaxBarValue, EditorStyles.boldLabel);
        automaticProp = barData.FindPropertyRelative("automaticMaxValue");
        automatic = automaticProp.boolValue;
        automatic = GUILayout.Toggle(automatic, "Auto");
        GUILayout.FlexibleSpace();
        automaticProp.boolValue = automatic;
        EditorGUILayout.EndHorizontal();
        if (automatic == false)
        {
            EditorGUILayout.PropertyField(maxProp);
            if (minProp.doubleValue > maxProp.doubleValue)
                maxProp.doubleValue = minProp.doubleValue + 0.1;
        }

        if (GUILayout.Button("Edit Values...") && mWindow == null)
            mWindow = ChartDataEditor.ShowForObject(serializedObject);
        //}
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();

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