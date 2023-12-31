﻿using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChartAndGraph
{
    internal class EditorMenu
    {
        private static void InstanciateCanvas(string path)
        {
            var canvases = Object.FindObjectsOfType<Canvas>();
            if (canvases == null || canvases.Length == 0)
            {
                EditorUtility.DisplayDialog("No canvas in scene", "Please add a canvas to the scene and try again",
                    "Ok");
                return;
            }

            Canvas canvas = null;
            foreach (var c in canvases)
                if (c.transform.parent == null)
                {
                    canvas = c;
                    break;
                }

            if (canvas == null)
            {
                EditorUtility.DisplayDialog("No canvas in scene", "Please add a canvas to the scene and try again",
                    "Ok");
                return;
            }

            var obj = Resources.Load<GameObject>(path);
            var newObj = Object.Instantiate(obj);
            newObj.transform.SetParent(canvas.transform, false);
            newObj.name = newObj.name.Replace("(Clone)", "");
            Undo.RegisterCreatedObjectUndo(newObj, "Create Object");
        }

        [MenuItem("Tools/Charts/Clear All")]
        public static void ClearChartGarbage()
        {
            var children = Object.FindObjectsOfType<ChartItem>();
            for (var i = 0; i < children.Length; ++i)
                if (children[i] != null)
                    ChartCommon.SafeDestroy(children[i].gameObject);
        }

        [MenuItem("Tools/Charts/Fix prefabs")]
        public static void ClearPrefabs()
        {
            if (EditorUtility.DisplayDialog("Warning", "Make sure to backup your project before calling Fix Prefabs",
                    "OK", "Cancel") == false)
                return;
            var prefabs = AssetDatabase.FindAssets("t:prefab");

            foreach (var guid in prefabs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                // Load the contents of the Prefab Asset.
                try
                {
                    var contentsRoot = PrefabUtility.LoadPrefabContents(path);
                    if (contentsRoot.GetComponentInChildren<AnyChart>() != null)
                    {
                        Debug.Log("Fixing " + path);
                        var save = false;
                        // Modify Prefab contents.
                        foreach (var item in contentsRoot.GetComponentsInChildren<ChartItem>())
                        {
                            if (item == null)
                                continue;
                            if (item.gameObject != null)
                            {
                                save = true;
                                Object.DestroyImmediate(item.gameObject);
                            }
                        }

                        if (save)
                        {
                            Debug.Log("Saving " + path);
                            PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
                        }
                        else
                        {
                            Debug.Log("No change to prefab " + path);
                        }
                    }

                    PrefabUtility.UnloadPrefabContents(contentsRoot);
                }
                catch (Exception)
                {
                    Debug.Log("Failed " + path);
                }
            }
        }

        [MenuItem("Tools/Charts/Radar/Canvas")]
        public static void AddRadarChartCanvas()
        {
            InstanciateCanvas("MenuPrefabs/2DRadar");
        }


        [MenuItem("Tools/Charts/Bar/Canvas/Simple")]
        public static void AddBarChartSimpleCanvas()
        {
            InstanciateCanvas("MenuPrefabs/BarCanvasSimple");
        }

        [MenuItem("Tools/Charts/Bar/Canvas/Multiple Groups")]
        public static void AddBarChartMultipleCanvas()
        {
            InstanciateCanvas("MenuPrefabs/BarCanvasMultiple");
        }


        [MenuItem("Tools/Charts/Torus/Canvas")]
        public static void AddTorusChartCanvas()
        {
            InstanciateCanvas("MenuPrefabs/TorusCanvas");
        }

        [MenuItem("Tools/Charts/Pie/Canvas")]
        public static void AddPieChartCanvas()
        {
            InstanciateCanvas("MenuPrefabs/PieCanvas");
        }


        [MenuItem("Tools/Charts/Graph/Canvas/Simple")]
        public static void AddGraphSimple()
        {
            InstanciateCanvas("MenuPrefabs/GraphSimple");
        }

        [MenuItem("Tools/Charts/Graph/Canvas/Multiple")]
        public static void AddGraphMultiple()
        {
            InstanciateCanvas("MenuPrefabs/GraphMultiple");
        }


        [MenuItem("Tools/Charts/Legend")]
        public static void AddChartLegend()
        {
            InstanciateCanvas("MenuPrefabs/Legend");
        }
    }
}