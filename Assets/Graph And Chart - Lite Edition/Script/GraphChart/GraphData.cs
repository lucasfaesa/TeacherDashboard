using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChartAndGraph
{
    [Serializable]
    public partial class GraphData : ScrollableChartData, IInternalGraphData
    {
        [SerializeField] private SerializedCategory[] mSerializedData = new SerializedCategory[0];

        private VectorComparer mComparer = new();
        private List<DoubleVector3> mTmpDriv = new();

        private bool IsExtended
        {
            get
            {
                var res = false;
                CheckExtended(ref res);
                return res;
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (CategoryData d in mData.Values)
                if (d.UpdateCurveAnimation())
                    RaiseRealtimeDataChanged(0, d.Name);
        }

        event Action<int, string> IInternalGraphData.InternalRealTimeDataChanged
        {
            add => RealtimeDataChanged += value;

            remove => RealtimeDataChanged -= value;
        }

        event EventHandler IInternalGraphData.InternalViewPortionChanged
        {
            add => ViewPortionChanged += value;
            remove => ViewPortionChanged -= value;
        }

        event EventHandler IInternalGraphData.InternalDataChanged
        {
            add => DataChanged += value;

            remove => DataChanged -= value;
        }


        double IInternalGraphData.GetMaxValue(int axis, bool dataValue)
        {
            return GetMaxValue(axis, dataValue);
        }

        double IInternalGraphData.GetMinValue(int axis, bool dataValue)
        {
            return GetMinValue(axis, dataValue);
        }

        public override void OnAfterDeserialize()
        {
            if (mSerializedData == null)
                return;
            mData.Clear();
            mSuspendEvents = true;
            for (var i = 0; i < mSerializedData.Length; i++)
            {
                var cat = mSerializedData[i];
                if (cat.Depth < 0)
                    cat.Depth = 0f;
                var name = cat.Name;
                AddInnerCategoryGraph(name, cat.LinePrefab, cat.Material, cat.LineThickness, cat.LineTiling,
                    cat.FillPrefab, cat.InnerFill, cat.StetchFill, cat.DotPrefab, cat.PointMaterial, cat.PointSize,
                    cat.Depth, cat.IsBezierCurve, cat.SegmentsPerCurve, cat.InitialData);
                Set2DCategoryPrefabs(name, cat.LineHoverPrefab, cat.PointHoverPrefab);
                var data = (CategoryData)mData[name];
                data.AllowNonFunctions = cat.AllowNonFunctionsBeta;
                data.ViewOrder = i;
                data.MaskPoints = cat.MaskPoints;
                if (data.Data == null)
                    data.Data = new List<DoubleVector3>();
                else
                    data.Data.Clear();

                if (cat.InitialData != null && cat.InitialData.Length > 0)
                    SetInitialData(name, cat.InitialData, cat.IsBezierCurve);
                else if (cat.data != null)
                    data.Data.AddRange(cat.data);

//                if (cat.data != null)
//                    data.Data.AddRange(cat.data);
                data.MaxX = cat.MaxX;
                data.MaxY = cat.MaxY;
                data.MinX = cat.MinX;
                data.MinY = cat.MinY;
                data.MaxRadius = cat.MaxRadius;
            }

            mSuspendEvents = false;
        }

        public override void OnBeforeSerialize()
        {
            var serialized = new List<SerializedCategory>();
            foreach (var pair in mData.Select(x =>
                         new KeyValuePair<string, CategoryData>(x.Key, (CategoryData)x.Value)))
            {
                var cat = new SerializedCategory();
                cat.Name = pair.Key;
                cat.MaxX = pair.Value.MaxX;
                cat.MinX = pair.Value.MinX;
                cat.MaxY = pair.Value.MaxY;
                cat.MaxRadius = pair.Value.MaxRadius;
                cat.MinY = pair.Value.MinY;
                cat.LineThickness = pair.Value.LineThickness;
                cat.StetchFill = pair.Value.StetchFill;
                cat.Material = pair.Value.LineMaterial;
                cat.LineHoverPrefab = pair.Value.LineHoverPrefab;
                cat.PointHoverPrefab = pair.Value.PointHoverPrefab;
                cat.LineTiling = pair.Value.LineTiling;
                cat.InnerFill = pair.Value.FillMaterial;
                cat.data = pair.Value.Data.ToArray();
                cat.PointSize = pair.Value.PointSize;
                cat.IsBezierCurve = pair.Value.IsBezierCurve;
                cat.AllowNonFunctionsBeta = pair.Value.AllowNonFunctions;
                cat.SegmentsPerCurve = pair.Value.SegmentsPerCurve;
                cat.PointMaterial = pair.Value.PointMaterial;
                cat.LinePrefab = pair.Value.LinePrefab;
                cat.Depth = pair.Value.Depth;
                cat.DotPrefab = pair.Value.DotPrefab;
                cat.FillPrefab = pair.Value.FillPrefab;
                cat.ViewOrder = pair.Value.ViewOrder;
                cat.InitialData = pair.Value.initialData;
                if (cat.Depth < 0)
                    cat.Depth = 0f;
                cat.MaskPoints = pair.Value.MaskPoints;
                serialized.Add(cat);
            }

            mSerializedData = serialized.OrderBy(x => x.ViewOrder).ToArray();
        }

        int IInternalGraphData.TotalCategories => mData.Count;

        IEnumerable<CategoryData> IInternalGraphData.Categories
        {
            get { return mData.Values.Select(x => (CategoryData)x).OrderBy(x => x.ViewOrder); }
        }

        partial void CheckExtended(ref bool result);

        public void AnimateCurve(string category, float time)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            if (data.IsBezierCurve == false)
            {
                Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
                return;
            }

            data.CurveAnimationCurrentTime = 0.0;
            data.CurveAnimationTotalTime = time;
        }

        public void ClearAndMakeBezierCurve(string category)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.IsBezierCurve = true;
            ClearCategory(category);
        }


        public void ClearAndMakeLinear(string category)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.IsBezierCurve = false;
            ClearCategory(category);
        }

        /// <summary>
        ///     rename a category. throws and exception on error
        /// </summary>
        /// <param name="prevName"></param>
        /// <param name="newName"></param>
        public void RenameCategory(string prevName, string newName)
        {
            if (prevName == newName)
                return;
            if (mData.ContainsKey(newName))
                throw new ArgumentException(string.Format("A category named {0} already exists", newName));
            var cat = (CategoryData)mData[prevName];
            mData.Remove(prevName);
            cat.Name = newName;
            mData.Add(newName, cat);
            RaiseDataChanged();
        }

        /// <summary>
        ///     Adds a new category to the graph chart. each category corrosponds to a graph line.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="material"></param>
        /// <param name="innerFill"></param>
        public void AddCategory(string category, Material lineMaterial, double lineThickness, MaterialTiling lineTiling,
            Material innerFill, bool strechFill, Material pointMaterial, double pointSize, bool maskPoints = false)
        {
            if (mData.ContainsKey(category))
                throw new ArgumentException(string.Format("A category named {0} already exists", category));
            var data = new CategoryData();
            mData.Add(category, data);
            data.Name = category;
            data.MaskPoints = maskPoints;
            data.LineMaterial = lineMaterial;
            data.LineHoverPrefab = null;
            data.PointHoverPrefab = null;
            data.FillMaterial = innerFill;
            data.LineThickness = lineThickness;
            data.LineTiling = lineTiling;
            data.StetchFill = strechFill;
            data.PointMaterial = pointMaterial;
            data.PointSize = pointSize;
            RaiseDataChanged();
        }

        public bool isCategoryEnabled(string category)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return false;
            }

            var data = (CategoryData)mData[category];
            return data.Enabled;
        }

        public void SetCategoryEnabled(string category, bool enabled)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.Enabled = enabled;
            RaiseDataChanged();
        }

        public object StoreCategory(string category)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return null;
            }

            var data = (CategoryData)mData[category];
            return data.Store();
        }

        public void RestoreCategory(string category, object store)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.Restore(store);
        }

        /// <summary>
        ///     used intenally , do not call
        /// </summary>
        /// <param name="cats"></param>
        public object[] StoreAllCategoriesinOrder()
        {
            return mData.Values.Where(x => x.ViewOrder >= 0).OrderBy(x => x.ViewOrder).Cast<object>().ToArray();
        }

        /// <summary>
        ///     this is a beta method that allows having paths drawn on the graph
        /// </summary>
        /// <param name="category"></param>
        /// <param name="AllowNonFunctions"></param>
        public void ClearAndSetAllowNonFunctions(string category, bool AllowNonFunctions)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.AllowNonFunctions = AllowNonFunctions;
            ClearCategory(category);
        }

        public void Set2DCategoryPrefabs(string category, ChartItemEffect lineHover, ChartItemEffect pointHover)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.LineHoverPrefab = lineHover;
            data.PointHoverPrefab = pointHover;
        }

        protected void AddInnerCategoryGraph(string category, PathGenerator linePrefab, Material lineMaterial,
            double lineThickness, MaterialTiling lineTiling, FillPathGenerator fillPrefab, Material innerFill,
            bool strechFill, GameObject pointPrefab, Material pointMaterial, double pointSize, double depth,
            bool isCurve, int segmentsPerCurve, Vector2[] initialData = null)
        {
            if (mData.ContainsKey(category))
                throw new ArgumentException(string.Format("A category named {0} already exists", category));
            if (depth < 0f)
                depth = 0f;
            var data = new CategoryData();
            mData.Add(category, data);
            data.Name = category;
            data.LineMaterial = lineMaterial;
            data.FillMaterial = innerFill;
            data.LineThickness = lineThickness;
            data.LineTiling = lineTiling;
            data.StetchFill = strechFill;
            data.PointMaterial = pointMaterial;
            data.PointSize = pointSize;
            data.LinePrefab = linePrefab;
            data.FillPrefab = fillPrefab;
            data.DotPrefab = pointPrefab;
            data.Depth = depth;
            data.IsBezierCurve = isCurve;
            data.SegmentsPerCurve = segmentsPerCurve;
            data.initialData = initialData;
            RaiseDataChanged();
        }

        private void SetInitialData(string category, Vector2[] initialData, bool isCurve)
        {
            if (initialData.Length == 0)
                return;
            if (isCurve)
            {
                var p = initialData[0];
                SetCurveInitialPoint(category, p.x, p.y);
                for (var i = 1; i < initialData.Length; i++)
                    AddLinearCurveToCategory(category, new DoubleVector2(initialData[i]));
                MakeCurveCategorySmooth(category);
            }
            else
            {
                for (var i = 0; i < initialData.Length; i++)
                {
                    var p = initialData[i];
                    AddPointToCategory(category, p.x, p.y);
                }
            }
        }

        /// <summary>
        ///     sets the line style for the category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="lineMaterial"></param>
        /// <param name="lineThickness"></param>
        /// <param name="lineTiling"></param>
        public void SetCategoryLine(string category, Material lineMaterial, double lineThickness,
            MaterialTiling lineTiling)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.LineMaterial = lineMaterial;
            data.LineThickness = lineThickness;
            data.LineTiling = lineTiling;
            RaiseDataChanged();
        }

        public void GetCategoryLine(string category, out Material lineMaterial, out double lineThickness,
            out MaterialTiling lineTiling)
        {
            if (mData.ContainsKey(category) == false)
            {
                lineMaterial = null;
                lineThickness = 0;
                lineTiling = new MaterialTiling();
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            lineMaterial = data.LineMaterial;
            lineThickness = data.LineThickness;
            lineTiling = data.LineTiling;
        }

        /// <summary>
        ///     removed a category from the DataSource. returnes true on success , or false if the category does not exist
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool RemoveCategory(string category)
        {
            mSliders.RemoveAll(x => ((Slider)x).category == category);
            return mData.Remove(category);
        }

        /// <summary>
        ///     sets the point style for the selected category. set material to null for no points
        /// </summary>
        /// <param name="category"></param>
        /// <param name="pointMaterial"></param>
        /// <param name="pointSize"></param>
        public void SetCategoryPoint(string category, Material pointMaterial, double pointSize)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.PointMaterial = pointMaterial;
            data.PointSize = pointSize;
            RaiseDataChanged();
        }

        public void GetCategoryPoint(string category, out Material pointMaterial, out double pointSize)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                pointMaterial = null;
                pointSize = 0.0;
                return;
            }

            var data = (CategoryData)mData[category];
            pointMaterial = data.PointMaterial;
            pointSize = data.PointSize;
        }

        /// <summary>
        ///     sets the fill style for the selected category.set the material to null for no fill
        /// </summary>
        /// <param name="category"></param>
        /// <param name="fillMaterial"></param>
        /// <param name="strechFill"></param>
        public void SetCategoryFill(string category, Material fillMaterial, bool strechFill)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.FillMaterial = fillMaterial;
            data.StetchFill = strechFill;
            RaiseDataChanged();
        }

        public void GetCategoryFill(string category, out Material fillMaterial, out bool strechFill)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                fillMaterial = null;
                strechFill = false;
                return;
            }

            var data = (CategoryData)mData[category];
            fillMaterial = data.FillMaterial;
            strechFill = data.StetchFill;
        }

        /// <summary>
        ///     clears all the data for the selected category
        /// </summary>
        /// <param name="category"></param>
        public void ClearCategory(string category)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            mSliders.RemoveAll(x => ((Slider)x).category == category);
            mData[category].MaxX = null;
            mData[category].MaxY = null;
            mData[category].MinX = null;
            mData[category].MinY = null;
            mData[category].MaxRadius = null;
            ((CategoryData)mData[category]).Data.Clear();
            ((CategoryData)mData[category]).Regenerate = true;
            RaiseDataChanged();
        }

        /// <summary>
        ///     adds a point to the category. having the point x,y values as dates
        ///     <param name="category"></param>
        ///     <param name="x"></param>
        ///     <param name="y"></param>
        public void AddPointToCategory(string category, DateTime x, DateTime y, double pointSize = -1f)
        {
            var xVal = ChartDateUtility.DateToValue(x);
            var yVal = ChartDateUtility.DateToValue(y);
            AddPointToCategory(category, xVal, yVal, pointSize);
        }


        /// <summary>
        ///     gets the last point for the specified category. returns false if the category is empty , otherwise returns true and
        ///     assigns the point to the "point" parameter
        /// </summary>
        /// <param name="category"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool GetLastPoint(string category, out DoubleVector3 point)
        {
            var data = (CategoryData)mData[category];
            var points = data.getPoints();
            point = DoubleVector3.zero;
            if (points.Count == 0)
                return false;
            var index = points.Count - 1;
            point = points[index];
            return true;
        }

        public DoubleVector3 GetPoint(string category, int index)
        {
            var data = (CategoryData)mData[category];
            var points = data.getPoints();
            if (points.Count == 0)
                return DoubleVector3.zero;
            if (index < 0)
                return points[0];
            if (index >= points.Count)
                return points[points.Count - 1];
            return points[index];
        }

        public int GetPointCount(string category)
        {
            var data = (CategoryData)mData[category];
            var points = data.getPoints();
            return points.Count;
        }

        public static void AddPointToCategoryWithLabel(GraphChartBase chart, string category, DateTime x, double y,
            double pointSize = -1, string xLabel = null, string yLabel = null)
        {
            AddPointToCategoryWithLabel(chart, category, ChartDateUtility.DateToValue(x), y, pointSize, xLabel, yLabel);
        }

        public static void AddPointToCategoryWithLabel(GraphChartBase chart, string category, double x, DateTime y,
            double pointSize = -1, string xLabel = null, string yLabel = null)
        {
            AddPointToCategoryWithLabel(chart, category, x, ChartDateUtility.DateToValue(y), pointSize, xLabel, yLabel);
        }

        public static void AddPointToCategoryWithLabel(GraphChartBase chart, string category, DateTime x, DateTime y,
            double pointSize = -1, string xLabel = null, string yLabel = null)
        {
            AddPointToCategoryWithLabel(chart, category, ChartDateUtility.DateToValue(x),
                ChartDateUtility.DateToValue(y), pointSize, xLabel, yLabel);
        }


        public static void AddPointToCategoryWithLabel(GraphChartBase chart, string category, double x, double y,
            double pointSize = -1, string xLabel = null, string yLabel = null)
        {
            var item = new DoubleVector3(x, y, 0.0);
            chart.VectorValueToStringMap[item] = new KeyValuePair<string, string>(xLabel, yLabel);
            chart.DataSource.AddPointToCategory(category, x, y, pointSize);
        }


        /// <summary>
        ///     adds a point to the category. having the point x value as date
        ///     <param name="category"></param>
        ///     <param name="x"></param>
        ///     <param name="y"></param>
        public void AddPointToCategory(string category, DateTime x, double y, double pointSize = -1f)
        {
            var xVal = ChartDateUtility.DateToValue(x);
            AddPointToCategory(category, xVal, y, pointSize);
        }

        /// <summary>
        ///     adds a point to the category. having the point y value as date
        /// </summary>
        /// <param name="category"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPointToCategory(string category, double x, DateTime y, double pointSize = -1f)
        {
            var yVal = ChartDateUtility.DateToValue(y);
            AddPointToCategory(category, x, yVal, pointSize);
        }


        public void SetCurveInitialPoint(string category, DateTime x, double y, double pointSize = -1f)
        {
            SetCurveInitialPoint(category, ChartDateUtility.DateToValue(x), y, pointSize);
        }

        public void SetCurveInitialPoint(string category, DateTime x, DateTime y, double pointSize = -1f)
        {
            SetCurveInitialPoint(category, ChartDateUtility.DateToValue(x), ChartDateUtility.DateToValue(y), pointSize);
        }

        public void SetCurveInitialPoint(string category, double x, DateTime y, double pointSize = -1f)
        {
            SetCurveInitialPoint(category, x, ChartDateUtility.DateToValue(y), pointSize);
        }

        public void SetCategoryViewOrder(string category, int viewOrder)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            data.ViewOrder = viewOrder;
            RaiseDataChanged();
        }

        public void SetCurveInitialPoint(string category, double x, double y, double pointSize = -1f)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            if (data.IsBezierCurve == false)
            {
                Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
                return;
            }

            if (data.Data.Count > 0)
            {
                Debug.LogWarning(
                    "Initial point already set for this category, call is ignored. Call ClearCategory to create a new curve");
                return;
            }

            data.Regenerate = true;
            if (data.MaxRadius.HasValue == false || data.MaxRadius.Value < pointSize)
                data.MaxRadius = pointSize;
            if (data.MaxX.HasValue == false || data.MaxX.Value < x)
                data.MaxX = x;
            if (data.MinX.HasValue == false || data.MinX.Value > x)
                data.MinX = x;
            if (data.MaxY.HasValue == false || data.MaxY.Value < y)
                data.MaxY = y;
            if (data.MinY.HasValue == false || data.MinY.Value > y)
                data.MinY = y;

            var sizedPoint = new DoubleVector3(x, y, pointSize);
            data.Data.Add(sizedPoint);
            RaiseDataChanged();
        }

        private double min3(double a, double b, double c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        private double max3(double a, double b, double c)
        {
            return Math.Max(a, Math.Max(b, c));
        }

        private DoubleVector2 max3(DoubleVector2 a, DoubleVector2 b, DoubleVector2 c)
        {
            return new DoubleVector2(max3(a.x, b.x, c.x), max3(a.y, b.y, c.y));
        }

        private DoubleVector2 min3(DoubleVector2 a, DoubleVector2 b, DoubleVector2 c)
        {
            return new DoubleVector2(min3(a.x, b.x, c.x), min3(a.y, b.y, c.y));
        }

        public void MakeCurveCategorySmoothCubic(string category)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            if (data.IsBezierCurve == false)
            {
                Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
                return;
            }

            var points = data.Data;
            data.Regenerate = true;
            mTmpDriv.Clear();
            for (var i = 0; i < points.Count; i += 3)
            {
                var prev = points[Mathf.Max(i - 3, 0)];
                var next = points[Mathf.Min(i + 3, points.Count - 1)];
                var diff = next - prev;
                mTmpDriv.Add(diff * 0.25f);
            }

            for (var i = 3; i < points.Count; i += 3)
            {
                var driv = i / 3;
                var ct1 = points[i - 3] + mTmpDriv[driv - 1];
                var ct2 = points[i] - mTmpDriv[driv];
                points[i - 2] = ct1;
                points[i - 1] = ct2;
            }

            RaiseDataChanged();
        }

        public void MakeCurveCategorySmooth(string category, float tensor = 0.25f)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            if (data.IsBezierCurve == false)
            {
                Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
                return;
            }

            var points = data.Data;
            data.Regenerate = true;
            mTmpDriv.Clear();
            for (var i = 0; i < points.Count; i += 3)
            {
                var prev = points[Mathf.Max(i - 3, 0)];
                var next = points[Mathf.Min(i + 3, points.Count - 1)];
                var diff = next - prev;
                mTmpDriv.Add(diff * tensor);
            }

            for (var i = 3; i < points.Count; i += 3)
            {
                var driv = i / 3;
                var ct1 = points[i - 3] + mTmpDriv[driv - 1];
                var ct2 = points[i] - mTmpDriv[driv];
                points[i - 2] = ct1;
                points[i - 1] = ct2;
            }

            RaiseDataChanged();
        }

        public void AddLinearCurveToCategory(string category, DoubleVector2 toPoint, double pointSize = -1f)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            if (data.IsBezierCurve == false)
            {
                Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
                return;
            }

            if (data.Data.Count == 0)
            {
                Debug.LogWarning(
                    "Initial not set for this category, call is ignored. Call SetCurveInitialPoint to create a new curve");
                return;
            }

            var points = data.Data;
            var last = points[points.Count - 1];
            var c1 = DoubleVector3.Lerp(last, toPoint.ToDoubleVector3(), 1f / 3f);
            var c2 = DoubleVector3.Lerp(last, toPoint.ToDoubleVector3(), 2f / 3f);
            AddCurveToCategory(category, c1.ToDoubleVector2(), c2.ToDoubleVector2(), toPoint, pointSize);
        }

        public void AddCurveToCategory(string category, DoubleVector2 controlPointA, DoubleVector2 controlPointB,
            DoubleVector2 toPoint, double pointSize = -1f)
        {
            if (!IsExtended && pointSize >= 0f)
            {
                Debug.LogError("Point sizes are not supported in the lite version of Graph and Chart");
                pointSize = -1f;
            }

            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            if (data.IsBezierCurve == false)
            {
                Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
                return;
            }

            if (data.Data.Count == 0)
            {
                Debug.LogWarning(
                    "Initial not set for this category, call is ignored. Call SetCurveInitialPoint to create a new curve");
                return;
            }

            var points = data.Data;
            if (points.Count > 0 && points[points.Count - 1].x > toPoint.x)
            {
                Debug.LogWarning(
                    "Curves must be added sequentialy according to the x axis. toPoint.x is smaller then the previous point x value");
                return;
            }

            data.Regenerate = true;
            var min = min3(controlPointA, controlPointB, toPoint);
            var max = max3(controlPointA, controlPointB, toPoint);

            if (data.MaxRadius.HasValue == false || data.MaxRadius.Value < pointSize)
                data.MaxRadius = pointSize;
            if (data.MaxX.HasValue == false || data.MaxX.Value < max.x)
                data.MaxX = max.x;
            if (data.MinX.HasValue == false || data.MinX.Value > min.x)
                data.MinX = min.x;
            if (data.MaxY.HasValue == false || data.MaxY.Value < max.y)
                data.MaxY = max.y;
            if (data.MinY.HasValue == false || data.MinY.Value > min.y)
                data.MinY = min.y;

            points.Add(controlPointA.ToDoubleVector3());
            points.Add(controlPointB.ToDoubleVector3());
            points.Add(new DoubleVector3(toPoint.x, toPoint.y, pointSize));

            RaiseDataChanged();
        }

        /// <summary>
        ///     adds a point to the category. The points are sorted by their x value automatically
        /// </summary>
        /// <param name="category"></param>
        /// <param name="point"></param>
        public void AddPointToCategory(string category, double x, double y, double pointSize = -1f)
        {
            if (!IsExtended && pointSize >= 0f)
            {
                Debug.LogError("Point sizes are not supported in the lite version of Graph and Chart");
                pointSize = -1f;
            }

            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            var data = (CategoryData)mData[category];
            if (data.IsBezierCurve)
            {
                Debug.LogWarning("Category is Bezier curve. use AddCurveToCategory instead ");
                return;
            }

            var point = new DoubleVector3(x, y, pointSize);

            var points = data.Data;

            if (data.MaxRadius.HasValue == false || data.MaxRadius.Value < pointSize)
                data.MaxRadius = pointSize;
            if (data.MaxX.HasValue == false || data.MaxX.Value < point.x)
                data.MaxX = point.x;
            if (data.MinX.HasValue == false || data.MinX.Value > point.x)
                data.MinX = point.x;
            if (data.MaxY.HasValue == false || data.MaxY.Value < point.y)
                data.MaxY = point.y;
            if (data.MinY.HasValue == false || data.MinY.Value > point.y)
                data.MinY = point.y;

            if (points.Count > 0)
                if (data.AllowNonFunctions || points[points.Count - 1].x <= point.x)
                {
                    points.Add(point);
                    RaiseDataChanged();
                    return;
                }

            //   points.Add(point);
            var search = points.BinarySearch(point, mComparer);
            if (search < 0)
                search = ~search;
            points.Insert(search, point);
            RaiseDataChanged();
        }

        protected override void AppendDatum(string category, MixedSeriesGenericValue value)
        {
            throw new NotImplementedException();
        }

        protected override void InnerClearCategory(string category)
        {
            throw new NotImplementedException();
        }

        protected override bool AddCategory(string category, BaseScrollableCategoryData data)
        {
            throw new NotImplementedException();
        }

        protected override void AppendDatum(string category, IList<MixedSeriesGenericValue> value)
        {
            throw new NotImplementedException();
        }

        public override BaseScrollableCategoryData GetDefaultCategory()
        {
            throw new NotImplementedException();
        }

        [Serializable]
        public class CategoryData : BaseScrollableCategoryData
        {
            private static List<DoubleVector3> mEmpty = new();

            public bool IsBezierCurve;
            public int SegmentsPerCurve = 10;
            public bool Regenerate = true;
            public bool AllowNonFunctions;
            public Vector2[] initialData;
            public double CurveAnimationTotalTime = -1.0;
            public double CurveAnimationCurrentTime;

            public ChartItemEffect LineHoverPrefab;
            public ChartItemEffect PointHoverPrefab;
            public bool MaskPoints;
            public Material LineMaterial;
            public MaterialTiling LineTiling;
            public double LineThickness = 1f;
            public Material FillMaterial;
            public bool StetchFill;
            public Material PointMaterial;
            public double PointSize = 5f;
            public GameObject DotPrefab;
            public double Depth;
            private double CurveAnimationFactor = 1;
            public List<DoubleVector3> Data = new();
            public FillPathGenerator FillPrefab;
            public PathGenerator LinePrefab;
            public List<DoubleVector3> mTmpCurveData = new();

            public bool UpdateCurveAnimation()
            {
                if (CurveAnimationTotalTime < 0.0)
                    return false;
                CurveAnimationCurrentTime += Time.deltaTime;
                CurveAnimationFactor = Math.Min(1.0, CurveAnimationCurrentTime / CurveAnimationTotalTime);
                if (CurveAnimationFactor >= 1.0)
                    CurveAnimationTotalTime = -1.0;
                Regenerate = true;
                return true;
            }

            public List<DoubleVector3> getPoints()
            {
                if (Enabled == false)
                    return mEmpty;
                if (IsBezierCurve == false)
                    return Data;
                if (Regenerate == false)
                    return mTmpCurveData;
                Regenerate = false;
                mTmpCurveData.Clear();
                if (Data.Count <= 0)
                    return mTmpCurveData;
                mTmpCurveData.Add(Data[0]);
                if (Data.Count < 4)
                    return mTmpCurveData;
                var endCount = Data.Count - 1;
                for (var i = 0; i < endCount; i += 3)
                {
                    var factor = Math.Min(1.0, ((double)i + 3) / (endCount - 1));
                    if (factor < CurveAnimationFactor)
                    {
                        AddInnerCurve(Data[i], Data[i + 1], Data[i + 2], Data[i + 3], 1.0);
                        mTmpCurveData.Add(Data[i + 3]);
                    }
                    else
                    {
                        var prevFactor = Math.Min(1.0, i / (double)(endCount - 1));
                        var blend = (CurveAnimationFactor - prevFactor) / (factor - prevFactor);
                        AddInnerCurve(Data[i], Data[i + 1], Data[i + 2], Data[i + 3], blend);
                        break;
                    }
                }

                return mTmpCurveData;
            }

            public void AddInnerCurve(DoubleVector3 p1, DoubleVector3 c1, DoubleVector3 c2, DoubleVector3 p2,
                double factor)
            {
                for (var i = 0; i < SegmentsPerCurve * factor; i++)
                {
                    var blend = i / (double)SegmentsPerCurve;
                    var invBlend = 1f - blend;
                    var p = invBlend * invBlend * invBlend * p1 + 3f * invBlend * invBlend * blend * c1 +
                            3f * blend * blend * invBlend * c2 + blend * blend * blend * p2;
                    mTmpCurveData.Add(new DoubleVector3(p.x, p.y, 0f));
                }
            }


            public object Store()
            {
                return MemberwiseClone();
            }

            public void Restore(object store)
            {
                var cat = (CategoryData)store;
                LineHoverPrefab = cat.LineHoverPrefab;
                PointHoverPrefab = cat.PointHoverPrefab;
                LineMaterial = cat.LineMaterial;
                LineTiling = cat.LineTiling;
                LineThickness = cat.LineThickness;
                FillMaterial = cat.FillMaterial;
                StetchFill = cat.StetchFill;
                PointMaterial = cat.PointMaterial;
                PointSize = cat.PointSize;
                LinePrefab = cat.LinePrefab;
                FillPrefab = cat.FillPrefab;
                DotPrefab = cat.DotPrefab;
                Depth = cat.Depth;
                IsBezierCurve = cat.IsBezierCurve;
                SegmentsPerCurve = cat.SegmentsPerCurve;
                AllowNonFunctions = cat.AllowNonFunctions;
            }
        }

        private class VectorComparer : IComparer<DoubleVector3>
        {
            public int Compare(DoubleVector3 x, DoubleVector3 y)
            {
                if (x.x < y.x)
                    return -1;
                if (x.x > y.x)
                    return 1;
                return 0;
            }
        }

        [Serializable]
        private class SerializedCategory
        {
            public string Name;
            public Vector2[] InitialData = new Vector2[0];
            public bool IsBezierCurve;
            public int SegmentsPerCurve = 10;

            [NonCanvasAttribute] public double Depth;

            public ChartItemEffect LineHoverPrefab;
            public ChartItemEffect PointHoverPrefab;
            public bool MaskPoints;
            public Material Material;
            public MaterialTiling LineTiling;
            public Material InnerFill;
            public double LineThickness = 1f;
            public bool StetchFill;

            [NonCanvasAttribute] public GameObject DotPrefab;

            public Material PointMaterial;
            public double PointSize;
            public int ViewOrder;

            [HideInInspector] public bool AllowNonFunctionsBeta;

            [HideInInspector] public DoubleVector3[] data;

            [NonCanvasAttribute] public FillPathGenerator FillPrefab;

            [NonCanvasAttribute] public PathGenerator LinePrefab;

            [HideInInspector] public double? MaxX, MaxY, MinX, MinY, MaxRadius;
        }

        private class Slider : BaseSlider
        {
            public Slider(GraphData parent)
            {
                mParent = parent;
            }

            public override string Category => category;

            public override DoubleVector2 Max => current.ToDoubleVector2();

            public override int MinIndex => from;

            public override DoubleVector2 Min => current.ToDoubleVector2();
#pragma warning disable 0649
#pragma warning disable 0414
            public string category;
            public int from;
            public DoubleVector3 To;
            public DoubleVector3 Base;
            public DoubleVector3 current;
            public int index;
            private GraphData mParent;
#pragma warning restore 0414
#pragma warning restore 0649
        }
    }
}