#define Graph_And_Chart_PRO
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChartAndGraph
{
    [CustomEditor(typeof(GraphChartBase), true)]
    internal class GraphChartInspector : Editor
    {
        private readonly HashSet<string> mAllNames = new();
        private GUIStyle mBold;
        private bool mCategories;
        private string mCategoryError;
        private string mEditedCategory = "";
        private string mNewCategoryName = "";
        private readonly Dictionary<string, string> mOperations = new();
        private GUIStyle mRedStyle;
        private RenameWindow mRenameWindow;
        private Texture mSettings;
        private GUIStyle mSplitter;
        private readonly List<int> mToRemove = new();
        private readonly List<int> mToUp = new();
        private GraphDataEditor mWindow;

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

            for (var i = 0; i < mToRemove.Count; i++) items.DeleteArrayElementAtIndex(mToRemove[i]);
            for (var i = 0; i < mToUp.Count; i++)
            {
                var cur = mToUp[i];
                items.MoveArrayElement(cur, cur - 1);
            }
        }

        private void DataEditor(SerializedProperty data, string type, string property, string caption)
        {
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
                            {
                                do
                                {
                                    if (entry.name != "Name" && entry.name != "data")
                                    {
                                        if (entry.name == "ViewOrder")
                                            continue;
                                        if (target is GraphChart) // canvas graph chart
                                        {
                                            if (entry.name == "LinePrefab" || entry.name == "FillPrefab" ||
                                                entry.name == "DotPrefab" || entry.name == "Depth") continue;
                                        }
                                        else
                                        {
                                            if (entry.name == "LineHoverPrefab" || entry.name == "PointHoverPrefab")
                                                continue;
                                        }

                                        EditorGUILayout.PropertyField(entry, true);
                                    }
                                } while (entry.Next(false) && SerializedProperty.EqualContents(entry, end) == false);

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()).xMin);
                                if (GUILayout.Button("Edit Values..."))
                                {
                                    mEditedCategory = name;
                                    if (mWindow == null)
                                        mWindow = GraphDataEditor.ShowForObject(serializedObject, mEditedCategory);
                                }

                                GUILayout.EndHorizontal();
                            }
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

        private bool RenameCategory(string oldName, string newName)
        {
            var graph = (GraphChartBase)serializedObject.targetObject;
            try
            {
                graph.DataSource.RenameCategory(oldName, newName);
            }
            catch (Exception)
            {
                return false;
            }

            serializedObject.Update();
            if (graph.gameObject.activeInHierarchy)
                graph.GenerateChart();
            else
                EditorUtility.SetDirty(graph);
            return true;
        }

        private void RenameCalled(object val)
        {
            var data = (KeyValuePair<string, string>)val;
            var window = EditorWindow.GetWindow<RenameWindow>();
            mRenameWindow = window;
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
        }

        public override void OnInspectorGUI()
        {
            //serializedObject.Update();
            DrawDefaultInspector();

            var autoScrollHorizontally = serializedObject.FindProperty("autoScrollHorizontally");
            var horizontalScrolling = serializedObject.FindProperty("horizontalScrolling");
            var autoScrollVertically = serializedObject.FindProperty("autoScrollVertically");
            var verticalScrolling = serializedObject.FindProperty("verticalScrolling");
            // SerializedProperty scrollable = serializedObject.FindProperty("scrollable");
            // EditorGUILayout.PropertyField(scrollable);
            //  if (scrollable.boolValue == true)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(autoScrollHorizontally);
                if (autoScrollHorizontally.boolValue == false)
                    EditorGUILayout.PropertyField(horizontalScrolling);
                EditorGUILayout.PropertyField(autoScrollVertically);
                if (autoScrollVertically.boolValue == false)
                    EditorGUILayout.PropertyField(verticalScrolling);
                EditorGUI.indentLevel--;
            }

            var graphData = serializedObject.FindProperty("Data");
            EditorGUILayout.BeginVertical();
            Splitter();
            if (mBold == null)
                mBold = new GUIStyle(EditorStyles.foldout);
            EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);


            EditorGUI.indentLevel++;
            NamedItemEditor(graphData, "category", "mSerializedData", "Categories", ref mCategoryError, ref mCategories,
                ref mNewCategoryName);
            EditorGUI.indentLevel--;

            var horizontalOrigin = graphData.FindPropertyRelative("horizontalViewOrigin");
            var horizontalSize = graphData.FindPropertyRelative("horizontalViewSize");
            var automaticcHorizontaViewGap = graphData.FindPropertyRelative("automaticcHorizontaViewGap");

            var verticalOrigin = graphData.FindPropertyRelative("verticalViewOrigin");
            var verticalSize = graphData.FindPropertyRelative("verticalViewSize");
            var automaticVerticalViewGap = graphData.FindPropertyRelative("automaticVerticalViewGap");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Horizontal View", EditorStyles.boldLabel);
            var automaticProp = graphData.FindPropertyRelative("automaticHorizontalView");
            EditorGUILayout.PropertyField(automaticProp, new GUIContent("Auto"));
            var automatic = automaticProp.boolValue;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (automatic == false)
            {
                EditorGUILayout.PropertyField(horizontalOrigin);
                EditorGUILayout.PropertyField(horizontalSize);
                //  if (horizontalSize.doubleValue < 0.0)
                //      horizontalSize.doubleValue = 0.0001;
            }
            else
            {
                EditorGUILayout.PropertyField(automaticcHorizontaViewGap, new GUIContent("Horizontal Gap"));
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Vertical View", EditorStyles.boldLabel);
            automaticProp = graphData.FindPropertyRelative("automaticVerticallView");
            EditorGUILayout.PropertyField(automaticProp, new GUIContent("Auto"));
            automatic = automaticProp.boolValue;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (automatic == false)
            {
                EditorGUILayout.PropertyField(verticalOrigin);
                EditorGUILayout.PropertyField(verticalSize);
                //       if (verticalSize.doubleValue < 0.0)
                //           verticalSize.doubleValue = 0.0001;
            }
            else
            {
                EditorGUILayout.PropertyField(automaticVerticalViewGap, new GUIContent("Vertical Gap"));
            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            if (mWindow != null)
            {
                mWindow.SetEditedObject(serializedObject, mEditedCategory);
                mWindow.Repaint();
            }
        }
    }
}