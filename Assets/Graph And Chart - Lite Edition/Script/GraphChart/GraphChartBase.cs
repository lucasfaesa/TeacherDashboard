#define Graph_And_Chart_PRO
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace ChartAndGraph
{
    /// <summary>
    ///     the graph chart class.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class GraphChartBase : ScrollableAxisChart
    {
        [SerializeField] [Tooltip("The height ratio of the chart")]
        protected float heightRatio = 300;

        [SerializeField] [Tooltip("The width ratio of the chart")]
        protected float widthRatio = 600;

        /// <summary>
        ///     occures when a point is clicked
        /// </summary>
        public GraphEvent PointClicked = new();

        /// <summary>
        ///     occurs when a point is hovered
        /// </summary>
        public GraphEvent PointHovered = new();

        /// <summary>
        ///     occurs when no point is hovered any longer
        /// </summary>
        public UnityEvent NonHovered = new();


        [SerializeField] private string itemFormat = "<?x>:<?y>";

        /// <summary>
        ///     the graph data
        /// </summary>
        [HideInInspector] [SerializeField] protected GraphData Data = new();

        protected Dictionary<string, int> mMinimumUpdateIndex = new();

        protected bool mRealtimeUpdateIndex;
        private readonly StringBuilder mTmpBuilder = new();

        /// <summary>
        ///     The height ratio of the chart
        /// </summary>
        [SerializeField]
        public float HeightRatio
        {
            get => heightRatio;
            set
            {
                heightRatio = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     The width ratio of the chart
        /// </summary>
        public float WidthRatio
        {
            get => widthRatio;
            set
            {
                widthRatio = value;
                Invalidate();
            }
        }

        public string ItemFormat
        {
            get => itemFormat;
            set
            {
                itemFormat = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Holds the graph chart data. including values and categories
        /// </summary>
        public GraphData DataSource => Data;

        protected override LegenedData LegendInfo
        {
            get
            {
                var data = new LegenedData();
                if (Data == null)
                    return data;
                foreach (var cat in ((IInternalGraphData)Data).Categories)
                {
                    var item = new LegenedData.LegenedItem();
                    item.Name = cat.Name;
                    if (cat.FillMaterial != null)
                        item.Material = cat.FillMaterial;
                    else if (cat.LineMaterial != null)
                        item.Material = cat.LineMaterial;
                    else
                        item.Material = cat.PointMaterial;
                    data.AddLegenedItem(item);
                }

                return data;
            }
        }

        protected override IChartData DataLink => Data;

        protected override bool SupportsCategoryLabels => false;

        protected override bool SupportsGroupLables => false;

        protected override bool SupportsItemLabels => true;

        protected override float TotalHeightLink => heightRatio;

        protected override float TotalWidthLink => widthRatio;


        protected override float TotalDepthLink => 0.0f;

        protected override void Start()
        {
            base.Start();
            if (ChartCommon.IsInEditMode == false) HookEvents();
            Invalidate();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (ChartCommon.IsInEditMode == false) HookEvents();
            Data.RestoreDataValues();
            Invalidate();
        }

        public abstract void ClearCache();

        private void HookEvents()
        {
            ((IInternalGraphData)Data).InternalDataChanged -= GraphChart_InternalDataChanged;
            ((IInternalGraphData)Data).InternalRealTimeDataChanged -= GraphChartBase_InternalRealTimeDataChanged;
            ;
            ((IInternalGraphData)Data).InternalViewPortionChanged -= GraphChartBase_InternalViewPortionChanged;

            ((IInternalGraphData)Data).InternalViewPortionChanged += GraphChartBase_InternalViewPortionChanged;
            ((IInternalGraphData)Data).InternalDataChanged += GraphChart_InternalDataChanged;
            ((IInternalGraphData)Data).InternalRealTimeDataChanged += GraphChartBase_InternalRealTimeDataChanged;
            ;
        }

        private void GraphChartBase_InternalViewPortionChanged(object sender, EventArgs e)
        {
            ViewPortionChanged();
        }

        protected abstract void ViewPortionChanged();

        private void GraphChartBase_InternalRealTimeDataChanged(int index, string category)
        {
            if (category == null) // full invalidtion
                mRealtimeUpdateIndex = false;
            if (mRealtimeUpdateIndex)
            {
                mRealtimeUpdateIndex = true;
                int minIndex;
                if (mMinimumUpdateIndex.TryGetValue(category, out minIndex) == false)
                    minIndex = int.MaxValue;
                if (minIndex > index)
                    minIndex = index;
                mMinimumUpdateIndex[category] = minIndex;
            }

            InvalidateRealtime();
        }

        protected void ClearRealtimeIndexdata()
        {
            mMinimumUpdateIndex.Clear();
            mRealtimeUpdateIndex = true;
        }

        public override void Invalidate()
        {
            base.Invalidate();
            mRealtimeUpdateIndex = false; // trigger a full invalidation
            mMinimumUpdateIndex.Clear();
        }

        private void GraphChart_InternalDataChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnLabelSettingChanged()
        {
            base.OnLabelSettingChanged();
            Invalidate();
        }

        protected override void OnAxisValuesChanged()
        {
            base.OnAxisValuesChanged();
            Invalidate();
        }

        protected override void OnLabelSettingsSet()
        {
            base.OnLabelSettingsSet();
            Invalidate();
        }

        protected DoubleVector4 TransformPoint(Rect viewRect, Vector3 point, DoubleVector2 min, DoubleVector2 range)
        {
            return ChartCommon.interpolateInRect(viewRect,
                new DoubleVector3((point.x - min.x) / range.x, (point.y - min.y) / range.y));
        }

        private void UpdateMinMax(DoubleVector3 point, ref double minX, ref double minY, ref double maxX,
            ref double maxY)
        {
            minX = Math.Min(minX, point.x);
            maxX = Math.Max(maxX, point.x);
            minY = Math.Min(minY, point.y);
            maxY = Math.Max(maxY, point.y);
        }

        protected override float GetScrollingRange(int axis)
        {
            var min = (float)((IInternalGraphData)Data).GetMinValue(axis, false);
            var max = (float)((IInternalGraphData)Data).GetMaxValue(axis, false);
            return max - min;
        }

        private Rect CreateUvRect(Rect completeRect, Rect lineRect)
        {
            if (completeRect.width < 0.0001f || completeRect.height < 0.0001f)
                return new Rect();
            if (float.IsInfinity(lineRect.xMax) || float.IsInfinity(lineRect.xMin) || float.IsInfinity(lineRect.yMin) ||
                float.IsInfinity(lineRect.yMax))
                return new Rect();

            var x = (lineRect.xMin - completeRect.xMin) / completeRect.width;
            var y = (lineRect.yMin - completeRect.yMin) / completeRect.height;

            var w = lineRect.width / completeRect.width;
            var h = lineRect.height / completeRect.height;

            return new Rect(x, y, w, h);
        }

        protected int ClipPoints(IList<DoubleVector3> points, List<DoubleVector4> res, out Rect uv)
        {
            double minX, minY, maxX, maxY, xScroll, yScroll, xSize, ySize, xOut;
            GetScrollParams(out minX, out minY, out maxX, out maxY, out xScroll, out yScroll, out xSize, out ySize,
                out xOut);

            var direction = 1.0;
            if (minX > maxX)
                direction = -1.0;
            var prevOut = false;
            var prevIn = false;

            double maxXLocal = double.MinValue,
                minXLocal = double.MaxValue,
                maxYLocal = double.MinValue,
                minYLocal = double.MaxValue;
            minX = double.MaxValue;
            minY = double.MaxValue;
            maxX = double.MinValue;
            maxY = double.MinValue;
            var refrenceIndex = 0;

            for (var i = 0; i < points.Count; i++)
            {
                var pOut = prevOut;
                var pIn = prevIn;
                prevOut = false;
                prevIn = false;
                var point = points[i];
                UpdateMinMax(points[i], ref minX, ref minY, ref maxX, ref maxY);

                if (point.x * direction < xScroll * direction ||
                    point.x * direction > xOut * direction) // || point.y < yScroll || point.y > yOut)
                {
                    prevOut = true;
                    if (pIn) res.Add(point.ToDoubleVector4());
                    //   uv = CreateUvRect(new Rect(minX, minY, maxX - minX, maxY - minY), new Rect(minXLocal, minYLocal, maxXLocal - minXLocal, maxYLocal - minYLocal));
                    //   return refrenceIndex;
                    if (pOut)
                        if (point.x * direction > xOut * direction && points[i - 1].x * direction < xScroll * direction)
                        {
                            UpdateMinMax(points[i - 1], ref minXLocal, ref minYLocal, ref maxXLocal, ref maxYLocal);
                            UpdateMinMax(point, ref minXLocal, ref minYLocal, ref maxXLocal, ref maxYLocal);
                            res.Add(points[i - 1].ToDoubleVector4());
                            res.Add(point.ToDoubleVector4());
                        }
                }
                else
                {
                    prevIn = true;
                    if (pOut)
                    {
                        refrenceIndex = i - 1;
                        UpdateMinMax(points[i - 1], ref minXLocal, ref minYLocal, ref maxXLocal, ref maxYLocal);
                        res.Add(points[i - 1].ToDoubleVector4());
                    }

                    UpdateMinMax(point, ref minXLocal, ref minYLocal, ref maxXLocal, ref maxYLocal);
                    res.Add(point.ToDoubleVector4());
                }
            }

            for (var i = 0; i < res.Count; i++)
            {
                var p = res[i];
                p.w = p.z;
                p.z = 0f;
                res[i] = p;
            }

            uv = CreateUvRect(new Rect((float)minX, (float)minY, (float)(maxX - minX), (float)(maxY - minY)),
                new Rect((float)minXLocal, (float)yScroll, (float)(maxXLocal - minXLocal), (float)ySize));
            return refrenceIndex;
        }

        protected void TransformPoints(IList<DoubleVector3> points, Rect viewRect, DoubleVector3 min, DoubleVector3 max)
        {
            var range = max - min;
            if (Math.Abs(range.x) <= 0.0001f || Math.Abs(range.y) < 0.0001f)
                return;
            var radiusMultiplier = Math.Min(viewRect.width / Math.Abs(range.x), viewRect.height / Math.Abs(range.y));
            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var res = ChartCommon.interpolateInRect(viewRect,
                    new DoubleVector3((point.x - min.x) / range.x, (point.y - min.y) / range.y));
                res.z = point.z * radiusMultiplier;
                points[i] = res.ToDoubleVector3();
            }
        }

        protected bool TransformPoints(IList<DoubleVector4> points, List<Vector4> output, Rect viewRect,
            DoubleVector3 min, DoubleVector3 max)
        {
            output.Clear();
            var range = max - min;
            if (Math.Abs(range.x) <= 0.0001f || Math.Abs(range.y) < 0.0001f)
                return false;
            var radiusMultiplier = Math.Min(viewRect.width / Math.Abs(range.x), viewRect.height / Math.Abs(range.y));
            var HasSize = false;
            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var res = ChartCommon.interpolateInRect(viewRect,
                    new DoubleVector3((point.x - min.x) / range.x, (point.y - min.y) / range.y));
                res.z = 0.0;
                res.w = point.w * radiusMultiplier;
                if (point.w > 0f)
                    HasSize = true;
                output.Add(res.ToVector4());
            }

            return HasSize;
        }

        protected override void ValidateProperties()
        {
            base.ValidateProperties();
            if (heightRatio < 0f)
                heightRatio = 0f;
            if (widthRatio < 0f)
                widthRatio = 0f;
        }

        protected override bool HasValues(AxisBase axis)
        {
            return true; //all axis have values in the graph chart
        }

        protected override double MaxValue(AxisBase axis)
        {
            if (axis == null)
                return 0.0;
            if (axis == mHorizontalAxis)
                return ((IInternalGraphData)Data).GetMaxValue(0, false);
            if (axis == mVerticalAxis)
                return ((IInternalGraphData)Data).GetMaxValue(1, false);
            return 0.0;
        }

        protected override double MinValue(AxisBase axis)
        {
            if (axis == null)
                return 0.0;
            if (axis == mHorizontalAxis)
                return ((IInternalGraphData)Data).GetMinValue(0, false);
            if (axis == mVerticalAxis)
                return ((IInternalGraphData)Data).GetMinValue(1, false);
            return 0.0;
        }

        protected override void OnItemHoverted(object userData)
        {
            base.OnItemHoverted(userData);
            var args = userData as GraphEventArgs;
            if (PointHovered != null)
                PointHovered.Invoke(args);
        }

        public string FormatItem(double x, double y)
        {
            var p = new DoubleVector3(x, y);
            var xFormat = StringFromAxisFormat(p, mHorizontalAxis, true);
            var yFormat = StringFromAxisFormat(p, mVerticalAxis, false);

            FormatItem(mTmpBuilder, xFormat, yFormat);
            return mTmpBuilder.ToString();
        }

        public string FormatItem(string x, string y)
        {
            FormatItem(mTmpBuilder, x, y);
            return mTmpBuilder.ToString();
        }

        protected void FormatItem(StringBuilder builder, string x, string y)
        {
            builder.Length = 0;
            builder.Append(itemFormat);
            builder.Replace("<?x>", x).Replace("<?y>", y).Replace("\\n", Environment.NewLine);
        }

        protected override void OnItemSelected(object userData)
        {
            base.OnItemSelected(userData);
            var args = userData as GraphEventArgs;
            if (PointClicked != null)
                PointClicked.Invoke(args);
        }

        /// <summary>
        ///     event arguments for a bar chart event
        /// </summary>
        public class GraphEventArgs
        {
            public GraphEventArgs(int index, Vector3 position, DoubleVector2 value, float magnitude, string category,
                string xString, string yString)
            {
                Position = position;
                Value = value;
                Category = category;
                XString = xString;
                YString = yString;
                Index = index;
                Magnitude = magnitude;
            }

            public float Magnitude { get; }
            public int Index { get; }
            public string XString { get; }
            public string YString { get; }
            public Vector3 Position { get; }
            public DoubleVector2 Value { get; }
            public string Category { get; }
            public string Group { get; private set; }
        }

        /// <summary>
        ///     a graph chart event
        /// </summary>
        [Serializable]
        public class GraphEvent : UnityEvent<GraphEventArgs>
        {
        }
    }
}