﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ChartAndGraph
{
    /// <summary>
    ///     holds common operations of the charting library
    /// </summary>
    public partial class ChartCommon
    {
        private static Material mDefaultMaterial;

        static ChartCommon()
        {
            DefaultIntComparer = new IntComparer();
            DefaultDoubleComparer = new DoubleComparer();
            DefaultDoubleVector3Comparer = new DoubleVector3Comparer();
        }

        internal static Material DefaultMaterial
        {
            get
            {
                if (mDefaultMaterial == null)
                {
                    mDefaultMaterial = new Material(Shader.Find("Standard"));
                    mDefaultMaterial.color = Color.blue;
                }

                return mDefaultMaterial;
            }
        }

        internal static bool IsInEditMode => Application.isPlaying == false && Application.isEditor;

        /// <summary>
        /// </summary>
        public static IEqualityComparer<int> DefaultIntComparer { get; }

        public static IEqualityComparer<double> DefaultDoubleComparer { get; }
        public static IEqualityComparer<DoubleVector3> DefaultDoubleVector3Comparer { get; }

        internal static float SmoothLerp(float from, float to, float factor)
        {
            return from * (1f - factor) + to * factor;
        }

        internal static GameObject CreateCanvasChartItem()
        {
            var obj = new GameObject("item", typeof(RectTransform));
            obj.AddComponent<ChartItem>();
            return obj;
        }

        internal static double Max(double? x, double y)
        {
            if (x.HasValue == false)
                return y;
            return Math.Max(x.Value, y);
        }

        internal static double Min(double? x, double y)
        {
            if (x.HasValue == false)
                return y;
            return Math.Min(x.Value, y);
        }

        internal static double Clamp(double val)
        {
            if (val > 1.0)
                return 1.0;
            if (val < 0.0)
                return 0.0;
            return val;
        }

        internal static GameObject CreateChartItem()
        {
            var obj = new GameObject();
            obj.AddComponent<ChartItem>();
            return obj;
        }

        internal static double normalizeInRangeX(double value, DoubleVector3 min, DoubleVector3 size)
        {
            return normalizeInRange(value, min.x, size.x);
        }

        internal static double normalizeInRangeY(double value, DoubleVector3 min, DoubleVector3 size)
        {
            return normalizeInRange(value, min.y, size.y);
        }

        internal static double normalizeInRange(double value, double min, double size)
        {
            return (value - min) / size;
        }

        internal static double interpolateInRectX(Rect rect, double x)
        {
            return rect.x + rect.width * x;
        }

        internal static double interpolateInRectY(Rect rect, double y)
        {
            return rect.y + rect.height * y;
        }

        internal static Rect RectFromCenter(float centerX, float sizeX, float top, float bottom)
        {
            var half = sizeX * 0.5f;
            return new Rect(centerX - half, top, sizeX, bottom - top);
        }

        internal static DoubleVector4 interpolateInRect(Rect rect, DoubleVector3 point)
        {
            var x = rect.x + rect.width * point.x;
            var y = rect.y + rect.height * point.y;
            return new DoubleVector4(x, y, point.z, 0.0);
        }

        internal static void HideObjectEditor(GameObject obj, bool hideMode)
        {
            obj.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
        }

        internal static void HideObject(GameObject obj, bool hideMode)
        {
            // return;
            if (IsInEditMode)
            {
                obj.tag = "EditorOnly";
                obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable |
                                HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                return;
            }

            if (hideMode == false)
            {
                obj.hideFlags = HideFlags.DontSaveInEditor;
                return;
            }

            obj.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector |
                            HideFlags.NotEditable;
        }

        internal static float GetAutoLength(AnyChart parent, ChartOrientation orientation)
        {
            return orientation == ChartOrientation.Vertical
                ? ((IInternalUse)parent).InternalTotalWidth
                : ((IInternalUse)parent).InternalTotalHeight;
        }

        internal static float GetAutoDepth(AnyChart parent, ChartOrientation orientation, ChartDivisionInfo info)
        {
            var depth = ((IInternalUse)parent).InternalTotalDepth;
            if (info.MarkDepth.Automatic == false)
                depth = info.MarkDepth.Value;
            return depth;
        }

        internal static float GetAutoLength(AnyChart parent, ChartOrientation orientation, ChartDivisionInfo info)
        {
            var length = orientation == ChartOrientation.Vertical
                ? ((IInternalUse)parent).InternalTotalWidth
                : ((IInternalUse)parent).InternalTotalHeight;
            if (info.MarkLength.Automatic == false)
                length = info.MarkLength.Value;
            return length;
        }

        public static Vector2 Perpendicular(Vector2 v)
        {
            return new Vector2(v.y, -v.x);
        }

        internal static bool SegmentIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2,
            out Vector2 intersection)
        {
            intersection = new Vector2();

            var dirA = a2 - a1;
            var dirB = b2 - b1;

            var dotA = Vector2.Dot(dirA, Perpendicular(dirB));
            if (dotA == 0)
                return false;

            var dirAB = b1 - a1;
            var t = Vector2.Dot(dirAB, Perpendicular(dirB)) / dotA;
            if (t < 0 || t > 1)
                return false;

            var s = Vector2.Dot(dirAB, Perpendicular(dirA)) / dotA;
            if (s < 0 || s > 1)
                return false;

            intersection = a1 + t * dirA;

            return true;
        }

        internal static Vector2 FromPolarRadians(float angleDeg, float radius)
        {
            var x = radius * Mathf.Cos(angleDeg);
            var y = radius * Mathf.Sin(angleDeg);
            return new Vector2(x, y);
        }

        internal static Vector2 FromPolar(float angleDeg, float radius)
        {
            angleDeg *= Mathf.Deg2Rad;
            var x = radius * Mathf.Cos(angleDeg);
            var y = radius * Mathf.Sin(angleDeg);
            return new Vector2(x, y);
        }

        internal static Rect FixRect(Rect r)
        {
            var x = r.x;
            var y = r.y;
            var width = r.width;
            var height = r.height;
            if (width < 0)
            {
                x = r.x + width;
                width = -width;
            }

            if (height < 0)
            {
                y = r.y + height;
                height = -height;
            }

            return new Rect(x, y, width, height);
        }

        /// <summary>
        ///     safely assigns a material to a renderer. if the material is null than the default material is set instead
        /// </summary>
        /// <param name="renderer">the renderer</param>
        /// <param name="material">the material</param>
        /// <returns>true if material is not null, false otherwise</returns>
        internal static bool SafeAssignMaterial(Renderer renderer, Material material, Material defualt)
        {
            var toSet = material;
            if (toSet == null)
            {
                toSet = defualt;
                if (toSet == null)
                    toSet = DefaultMaterial;
            }

            renderer.sharedMaterial = toSet;
            return material != null;
        }

        /// <summary>
        /// </summary>
        /// <param name="newMesh"></param>
        /// <param name="cleanMesh"></param>
        internal static void CleanMesh(Mesh newMesh, ref Mesh cleanMesh)
        {
            if (cleanMesh == newMesh)
                return;
            if (cleanMesh != null)
                SafeDestroy(cleanMesh);
            cleanMesh = newMesh;
        }

        //#if UNITY_2018_3_OR_NEWER
        //#if UNITY_EDITOR
        //        public static void SafeDestroy(GameObject obj)
        //        {
        //            if (obj == null)
        //                return;
        //            if (Application.isEditor && Application.isPlaying == false)
        //            {
        //                if (UnityEditor.PrefabUtility.GetPrefabAssetType(obj) != UnityEditor.PrefabAssetType.NotAPrefab)
        //                {
        //                    if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(obj))
        //                    {
        //                        var parent = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(obj);
        //                        UnityEditor.PrefabUtility.UnpackPrefabInstance(parent, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
        //                    }
        //                }
        //                UnityEngine.Object.DestroyImmediate(obj);
        //            }
        //            else
        //                UnityEngine.Object.Destroy(obj);
        //        }
        //#endif
        //#endif

        public static void SafeDestroy(Object obj)
        {
            if (obj == null)
                return;
            if (Application.isEditor && Application.isPlaying == false)
                Object.DestroyImmediate(obj);
            else
                Object.Destroy(obj);
        }

        internal static UIVertex CreateVertex(Vector3 pos, Vector2 uv)
        {
            return CreateVertex(pos, uv, pos.z);
        }

        internal static UIVertex CreateVertex(Vector3 pos, Vector2 uv, float z)
        {
            var vert = new UIVertex();
            vert.color = Color.white;
            vert.uv0 = uv;
            pos.z = z;
            vert.position = pos;
            return vert;
        }

        internal static float GetTiling(MaterialTiling tiling)
        {
            if (tiling.EnableTiling == false)
                return -1f;
            return tiling.TileFactor;
        }

        internal static void FixBillboardText(ItemLabelsBase labels, BillboardText text)
        {
            var sharpness = Mathf.Clamp(labels.FontSharpness, 1f, 3f);
            SetTextParams(text.UIText, null, labels.FontSize, sharpness);
            text.Scale = 1f / sharpness;
            text.UIText.transform.localScale = new Vector3(text.Scale, text.Scale);
        }

        internal static T EnsureComponent<T>(GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (comp == null)
                comp = obj.AddComponent<T>();
            return comp;
        }


        private static float DotProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = b - a;
            var bc = c - b;
            return Vector2.Dot(ab, bc);
        }

        private static float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = b - a;
            var ac = c - a;
            return ab.x * ac.y - ab.y * ac.x;
        }

        internal static float SegmentPointSqrDistance(Vector2 a, Vector2 b, Vector2 point)
        {
            var dot = DotProduct(a, b, point);

            if (dot > 0)
                return (b - point).sqrMagnitude;

            dot = DotProduct(b, a, point);
            if (dot > 0)
                return (a - point).sqrMagnitude;

            var cross = CrossProduct(a, b, point);
            return cross * cross / (a - b).sqrMagnitude;
        }

        /* internal static BillboardText CreateBillboardText(Text prefab, Transform parentTransform, string text, float x, float y, float z, float angle, bool hideHirarechy, int fontSize, float sharpness)
         {
             return CreateBillboardText(prefab, parentTransform, text, x, y, z, angle, null, hideHirarechy, fontSize, sharpness);
         }*/
        internal static Vector3 LineCrossing(Vector3 v1, Vector3 v2, float horizontal)
        {
            if (Math.Abs(v1.x - v2.x) < 0.0001f)
                return new Vector3(v1.x, horizontal, v1.z);
            if (Math.Abs(v1.y - v2.y) < 0.0001f)
                return v1;
            var slope = (v1.y - v2.y) / (v1.x - v2.x);
            var n = v1.y - v1.x * slope;
            var x = (horizontal - n) / slope;
            var y = slope * x + n;
            return new Vector3(x, y, v1.z);
        }

        internal static BillboardText UpdateBillboardText(BillboardText billboardText, Transform parentTransform,
            string text, float x, float y, float z, float angle, Transform relativeFrom, bool hideHirarechy,
            bool yMirror)
        {
            var UIText = billboardText.UIText.gameObject;
            var billboard = billboardText.gameObject;
            HideObject(billboard, hideHirarechy);
            var direction = UIText.GetComponent<TextDirection>();
            var TextObj = billboardText.UIText;
            billboardText.YMirror = yMirror;
            if (direction != null)
            {
                TextObj = direction.Text.gameObject;
                if (relativeFrom != null)
                    direction.SetRelativeTo(relativeFrom, billboard.transform);
                //  else
                direction.SetDirection(angle);
            }

            // if (parentTransform != null && billboard.transform.parent != parentTransform)
            // {
            //    billboard.transform.SetParent(parentTransform, false);
            // }
            UpdateTextParams(TextObj, text);
            billboard.transform.localPosition = new Vector3(x, y, z);
            return billboardText;
        }

        internal static BillboardText CreateBillboardText(BillboardText item, MonoBehaviour prefab,
            Transform parentTransform, string text, float x, float y, float z, float angle, Transform relativeFrom,
            bool hideHirarechy, int fontSize, float sharpness)
        {
            if (item != null)
                return UpdateBillboardText(item, parentTransform, text, x, y, z, angle, relativeFrom, hideHirarechy,
                    prefab != null && prefab.transform.localScale.y < 0);
            if (prefab == null || prefab.gameObject == null)
            {
                var g = Resources.Load("Chart And Graph/DefaultText") as GameObject;
                prefab = g.GetComponent<Text>();
            }

            var UIText = Object.Instantiate(prefab.gameObject);
            var billboard = new GameObject();
            HideObject(billboard, hideHirarechy);
            if (parentTransform.GetComponent<RectTransform>())
                billboard.AddComponent<RectTransform>();

            if (parentTransform != null)
            {
                billboard.transform.SetParent(parentTransform, false);
                UIText.transform.SetParent(parentTransform, false);
            }

            var billboardText = billboard.AddComponent<BillboardText>();
            billboard.AddComponent<ChartItem>();
            var direction = UIText.GetComponent<TextDirection>();

            GameObject obj = null;

            if (direction != null)
            {
                obj = direction.Text.gameObject;
                if (relativeFrom != null)
                    direction.SetRelativeTo(relativeFrom, billboard.transform);
                //  else
                direction.SetDirection(angle);
            }

            if (obj == null)
                obj = UIText;
            sharpness = Mathf.Clamp(sharpness, 1f, 3f);
            var setParams = SetTextParams(obj, text, fontSize, sharpness);

            if (billboardText == null || setParams == false)
            {
                SafeDestroy(UIText);
                SafeDestroy(billboard);
                return null;
            }


            if (prefab != null && prefab.transform.localScale.y < 0)
                billboardText.YMirror = true;
            billboardText.Scale = 1f / sharpness;
            billboardText.UIText = obj;
            billboardText.Direction = direction;
            if (direction != null)
                billboardText.RectTransformOverride = direction.GetComponent<RectTransform>();
            else
                billboardText.RectTransformOverride = null;
            billboard.transform.localPosition = new Vector3(x, y, z);
            return billboardText;
        }

        static partial void DoTextSignInternal(MonoBehaviour Text, double sign);
        static partial void DoTextSignInternal3D(MonoBehaviour Text, double sign);

        internal static void DoTextSign(MonoBehaviour Text, double sign)
        {
            DoTextSignInternal(Text, sign);
            DoTextSignInternal3D(Text, sign);
        }

        static partial void GetTextInternal(GameObject obj, ref string text);
        static partial void GetTextInternal3D(GameObject obj, ref string text);


        internal static string GetText(GameObject obj)
        {
            var t = obj.GetComponent<Text>();
            if (t != null)
                return t.text;
            string res = null;
            GetTextInternal(obj, ref res);
            if (res == null)
                GetTextInternal3D(obj, ref res);
            return res;
        }

        static partial void UpdateTextParamsInternal(GameObject obj, string text);
        static partial void UpdateTextParamsInternal3D(GameObject obj, string text);

        internal static void UpdateTextParams(GameObject obj, string text)
        {
            var t = obj.GetComponent<Text>();
            if (t != null)
                t.text = text;
            UpdateTextParamsInternal(obj, text);
            UpdateTextParamsInternal3D(obj, text);
        }

        internal static void MakeMaskable(GameObject obj, bool masksable)
        {
            foreach (var g in obj.GetComponents<MaskableGraphic>())
                g.maskable = masksable;
        }

        static partial void SetTextParamsInternal(GameObject obj, string text, int fontSize, float sharpness,
            ref bool res);

        static partial void SetTextParamsInternal3D(GameObject obj, string text, int fontSize, float sharpness,
            ref bool res);

        internal static bool SetTextParams(GameObject obj, string text, int fontSize, float sharpness)
        {
            var t = obj.GetComponent<Text>();
            if (t != null)
            {
                t.fontSize = (int)(fontSize * sharpness);
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                t.resizeTextForBestFit = false;
                if (text != null)
                    t.text = text;
                return true;
            }

            var res = false;
            SetTextParamsInternal(obj, text, fontSize, sharpness, ref res);
            if (res == false)
                SetTextParamsInternal3D(obj, text, fontSize, sharpness, ref res);
            if (res == false)
                Debug.LogError(
                    "Text prefab must be either UI.Text or TextMeshPro object. For text mesh pro , you need to import the support package first. worldSpace text meshpro can only be used with non canvas charts. Check out the following link for more info http://prosourcelabs.com/Tutorials/TextMeshPro/IntegratingTextMeshPro.html");
            return res;
        }

        private class IntComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return x == y;
            }

            public int GetHashCode(int obj)
            {
                return obj.GetHashCode();
            }
        }

        private class DoubleComparer : IEqualityComparer<double>
        {
            public bool Equals(double x, double y)
            {
                return x == y;
            }

            public int GetHashCode(double obj)
            {
                return obj.GetHashCode();
            }
        }

        private class DoubleVector3Comparer : IEqualityComparer<DoubleVector3>
        {
            public bool Equals(DoubleVector3 x, DoubleVector3 y)
            {
                return x == y;
            }

            public int GetHashCode(DoubleVector3 obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}