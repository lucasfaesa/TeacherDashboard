#define Graph_And_Chart_PRO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    public class GraphChart : GraphChartBase, ICanvas
    {
        /// <summary>
        ///     occures when a point is clicked
        /// </summary>
        public GraphEvent LineClicked = new();

        /// <summary>
        ///     occurs when a point is hovered
        /// </summary>
        public GraphEvent LineHovered = new();


        [SerializeField] private bool fitToContainer;

        [SerializeField] private bool negativeFill;

        [SerializeField] private ChartMagin fitMargin;

        // [SerializeField]
        private bool enableBetaOptimization; // this features is not ready yet
        private readonly Dictionary<string, CategoryObject> mCategoryObjects = new();
        private readonly List<DoubleVector4> mClipped = new();
        private readonly HashSet<string> mOccupiedCateogies = new();
        private readonly StringBuilder mRealtimeStringBuilder = new();
        private readonly List<DoubleVector3> mTmpData = new();
        private readonly List<int> mTmpToRemove = new();
        private readonly List<Vector4> mTransformed = new();
        private readonly bool SupressRealtimeGeneration = false;

        public bool FitToContainer
        {
            get => fitToContainer;
            set
            {
                fitToContainer = value;
                OnPropertyUpdated();
            }
        }

        public bool NegativeFill
        {
            get => negativeFill;
            set
            {
                negativeFill = value;
                OnPropertyUpdated();
            }
        }

        public ChartMagin FitMargin
        {
            get => fitMargin;
            set
            {
                fitMargin = value;
                OnPropertyUpdated();
            }
        }

        private bool EnableBetaOptimization
        {
            get => enableBetaOptimization;
            set
            {
                enableBetaOptimization = value;
                OnPropertyUpdated();
            }
        }

        protected override ChartMagin MarginLink => FitMargin;

        public override bool IsCanvas => true;

        public override bool SupportRealtimeGeneration => true;

        protected override bool ShouldFitCanvas => true;

        protected override FitType FitAspectCanvas => FitType.Aspect;

        protected override void Update()
        {
            base.Update();
        }

        protected void OnLineSelected(object userData)
        {
            var args = userData as GraphEventArgs;
            if (LineClicked != null)
                LineClicked.Invoke(args);
            //   AddOccupiedCategory(args.Category, "line");
        }

        protected void OnLineHovered(object userData)
        {
            var args = userData as GraphEventArgs;
            if (LineHovered != null)
                LineHovered.Invoke(args);
            AddOccupiedCategory(args.Category, "line");
        }

        private void CenterObject(GameObject obj, RectTransform parent)
        {
            var t = obj.GetComponent<RectTransform>();
            t.SetParent(parent.transform, false);
            t.localScale = new Vector3(1f, 1f, 1f);
            t.anchorMin = new Vector2(0f, 0f);
            t.anchorMax = new Vector2(0f, 0f);
            t.sizeDelta = parent.sizeDelta;
            t.anchoredPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }

        private CanvasLines CreateDataObject(GraphData.CategoryData data, GameObject rectMask, bool mask)
        {
            var obj = new GameObject("Lines", typeof(RectTransform));
            ChartCommon.HideObject(obj, hideHierarchy);
            obj.AddComponent<ChartItem>();

            var rend = obj.AddComponent<CanvasRenderer>();
            rend.cullTransparentMesh = false;

            CenterObject(obj, rectMask.GetComponent<RectTransform>());
            //  Canvas c = obj.AddComponent<Canvas>();

            //c.pixelPerfect = false;
            //obj.AddComponent<GraphicRaycaster>();
            var lines = obj.AddComponent<CanvasLines>();
            lines.maskable = true;

            return lines;
        }

        protected override void ClearChart()
        {
            base.ClearChart();
            ClearBillboard();
            mActiveTexts.Clear();
            mCategoryObjects.Clear();
            ClearRealtimeIndexdata();
        }

        public override void ClearCache()
        {
            mCategoryObjects.Clear();
        }

        protected override double GetCategoryDepth(string category)
        {
            return 0.0;
        }

        private double AddRadius(double radius, double mag, double min, double max)
        {
            var size = max - min;
            var factor = size / mag;
            return factor * radius;
        }

        protected override void ViewPortionChanged()
        {
            InvalidateRealtime();
        }

        public override void GenerateRealtime()
        {
            if (SupressRealtimeGeneration)
                return;
            base.GenerateRealtime();

            var minX = ((IInternalGraphData)Data).GetMinValue(0, false);
            var minY = ((IInternalGraphData)Data).GetMinValue(1, false);
            var maxX = ((IInternalGraphData)Data).GetMaxValue(0, false);
            var maxY = ((IInternalGraphData)Data).GetMaxValue(1, false);

            var xScroll = GetScrollOffset(0);
            var yScroll = GetScrollOffset(1);
            var xSize = maxX - minX;
            var ySize = maxY - minY;
            var xOut = minX + xScroll + xSize;
            var yOut = minY + yScroll + ySize;

            var min = new DoubleVector3(xScroll + minX, yScroll + minY);
            var max = new DoubleVector3(xOut, yOut);

            var viewRect = new Rect(0f, 0f, widthRatio, heightRatio);

            var parentT = transform;

            if (mFixPosition != null)
                parentT = mFixPosition.transform;

            ClearBillboardCategories();

            foreach (var data in ((IInternalGraphData)Data).Categories)
            {
                CategoryObject obj = null;

                if (mCategoryObjects.TryGetValue(data.Name, out obj) == false)
                    continue;

                var minUpdateIndex = 0;
                if (mRealtimeUpdateIndex)
                    if (mMinimumUpdateIndex.TryGetValue(data.Name, out minUpdateIndex) == false)
                        minUpdateIndex = int.MaxValue;
                mClipped.Clear();
                mTmpData.Clear();

                mTmpData.AddRange(data.getPoints());
                Rect uv; // = new Rect(0f, 0f, 1f, 1f);
                var refrenceIndex = ClipPoints(mTmpData, mClipped, out uv);
                //mClipped.AddRange(mTmpData);
                TransformPoints(mClipped, mTransformed, viewRect, min, max);
                mTmpToRemove.Clear();
                var range = refrenceIndex + mClipped.Count;
                foreach (var key in obj.mCahced.Keys)
                    if (key < refrenceIndex || key > range)
                        mTmpToRemove.Add(key);

                for (var i = 0; i < mTmpToRemove.Count; i++)
                    obj.mCahced.Remove(mTmpToRemove[i]);

                obj.mCahced.Remove(mTmpData.Count -
                                   1); // never store the last point cache , it might be intepolating by the realtime feature
                obj.mCahced.Remove(mTmpData.Count -
                                   2); // never store the last point cache , it might be intepolating by the realtime feature

                if (mTmpData.Count == 0)
                    continue;
                if (mItemLabels != null && mItemLabels.isActiveAndEnabled && obj.mItemLabels != null)
                {
                    var textRect = viewRect;
                    textRect.xMin -= 1f;
                    textRect.yMin -= 1f;
                    textRect.xMax += 1f;
                    textRect.yMax += 1f;

                    var m = obj.mItemLabels;
                    m.Clear();

                    for (var i = 0; i < mTransformed.Count; i++)
                    {
                        if (mTransformed[i].w == 0.0)
                            continue;
                        var labelPos = (Vector3)mTransformed[i] + new Vector3(mItemLabels.Location.Breadth,
                            mItemLabels.Seperation, mItemLabels.Location.Depth);
                        if (mItemLabels.Alignment == ChartLabelAlignment.Base)
                            labelPos.y -= mTransformed[i].y;
                        if (textRect.Contains((Vector2)mTransformed[i]) == false)
                            continue;
                        (DoubleVector3, string) toSet;
                        var pointIndex = i + refrenceIndex;
                        if (obj.mCahced.TryGetValue(pointIndex, out toSet) == false ||
                            toSet.Item1 != mTmpData[i + refrenceIndex])
                        {
                            var pointValue = mTmpData[i + refrenceIndex];
                            var xFormat = StringFromAxisFormat(pointValue, mHorizontalAxis, mItemLabels.FractionDigits,
                                true);
                            var yFormat = StringFromAxisFormat(pointValue, mVerticalAxis, mItemLabels.FractionDigits,
                                false);
                            FormatItem(mRealtimeStringBuilder, xFormat, yFormat);
                            var formatted = mRealtimeStringBuilder.ToString();
                            mItemLabels.TextFormat.Format(mRealtimeStringBuilder, formatted, data.Name, "");
                            toSet.Item2 = mRealtimeStringBuilder.ToString();
                            obj.mCahced[pointIndex] = toSet;
                        }

                        labelPos -= new Vector3(CanvasFitOffset.x * TotalWidth, CanvasFitOffset.y * TotalHeight, 0f);
                        var billboard = m.AddText(this, mItemLabels.TextPrefab, parentT, mItemLabels.FontSize,
                            mItemLabels.FontSharpness, toSet.Item2, labelPos.x, labelPos.y, labelPos.z, 0f, null);
                        AddBillboardText(data.Name, i + refrenceIndex, billboard);
                    }

                    m.DestoryRecycled();
                    if (m.TextObjects != null)
                        foreach (var text in m.TextObjects)
                            ((IInternalUse)this).InternalTextController.AddText(text);
                }

                if (obj.mDots != null)
                {
                    var pickRect = viewRect;
                    var halfSize = (float)(data.PointSize * 0.5f);
                    pickRect.xMin -= halfSize;
                    pickRect.yMin -= halfSize;
                    pickRect.xMax += halfSize;
                    pickRect.yMax += halfSize;
                    obj.mDots.SetViewRect(pickRect, uv);
                    obj.mDots.ModifyLines(minUpdateIndex, mTransformed);
                    obj.mDots.SetRefrenceIndex(refrenceIndex);
                }

                if (obj.mLines != null)
                {
                    var tiling = 1f;
                    if (data.LineTiling.EnableTiling && data.LineTiling.TileFactor > 0f)
                    {
                        var length = 0f;
                        for (var i = 1; i < mTransformed.Count; i++)
                            length += (mTransformed[i - 1] - mTransformed[i]).magnitude;
                        tiling = length / data.LineTiling.TileFactor;
                    }

                    if (tiling <= 0.0001f)
                        tiling = 1f;
                    obj.mLines.Tiling = tiling;
                    obj.mLines.SetViewRect(viewRect, uv);
                    obj.mLines.ModifyLines(minUpdateIndex, mTransformed);
                    obj.mLines.SetRefrenceIndex(refrenceIndex);
                }

                if (obj.mFill != null)
                {
                    var zero = (float)(-min.y / (max.y - min.y) * viewRect.height);
                    if (negativeFill == false)
                        zero = 0f;
                    obj.mFill.SetFillZero(zero);
                    obj.mFill.SetViewRect(viewRect, uv);
                    obj.mFill.ModifyLines(minUpdateIndex, mTransformed);
                    obj.mFill.SetRefrenceIndex(refrenceIndex);
                }
            }

            ClearRealtimeIndexdata();
        }

        public override void InternalGenerateChart()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            base.InternalGenerateChart();

            if (FitToContainer)
            {
                var trans = GetComponent<RectTransform>();
                widthRatio = trans.rect.width;
                heightRatio = trans.rect.height;
            }

            ClearChart();

            if (Data == null)
                return;

            GenerateAxis(true);
            var minX = ((IInternalGraphData)Data).GetMinValue(0, false);
            var minY = ((IInternalGraphData)Data).GetMinValue(1, false);
            var maxX = ((IInternalGraphData)Data).GetMaxValue(0, false);
            var maxY = ((IInternalGraphData)Data).GetMaxValue(1, false);

            var xScroll = GetScrollOffset(0);
            var yScroll = GetScrollOffset(1);
            var xSize = maxX - minX;
            var ySize = maxY - minY;
            var xOut = minX + xScroll + xSize;
            var yOut = minY + yScroll + ySize;

            var min = new DoubleVector3(xScroll + minX, yScroll + minY);
            var max = new DoubleVector3(xOut, yOut);

            var viewRect = new Rect(0f, 0f, widthRatio, heightRatio);

            var index = 0;
            var total = ((IInternalGraphData)Data).TotalCategories + 1;
            var edit = false;
            ClearBillboard();
            mActiveTexts.Clear();

            var mask = CreateRectMask(viewRect);

            foreach (var data in ((IInternalGraphData)Data).Categories)
            {
                mClipped.Clear();
                var points = data.getPoints().ToArray();
                Rect uv;
                var refrenceIndex = ClipPoints(points, mClipped, out uv);
                TransformPoints(mClipped, mTransformed, viewRect, min, max);

                if (points.Length == 0 && ChartCommon.IsInEditMode)
                {
                    edit = true;
                    var tmpIndex = total - 1 - index;
                    var y1 = tmpIndex / (float)total;
                    var y2 = ((float)tmpIndex + 1) / total;

                    var pos1 = ChartCommon.interpolateInRect(viewRect, new DoubleVector3(0f, y1, -1f))
                        .ToDoubleVector3();
                    var pos2 = ChartCommon.interpolateInRect(viewRect, new DoubleVector3(0.5f, y2, -1f))
                        .ToDoubleVector3();
                    var pos3 = ChartCommon.interpolateInRect(viewRect, new DoubleVector3(1f, y1, -1f))
                        .ToDoubleVector3();

                    points = new[] { pos1, pos2, pos3 };
                    mTransformed.AddRange(points.Select(x => (Vector4)x.ToVector3()));
                    index++;
                }

                var list = new List<CanvasLines.LineSegement>();
                list.Add(new CanvasLines.LineSegement(mTransformed));
                var categoryObj = new CategoryObject();

                if (data.FillMaterial != null)
                {
                    var fill = CreateDataObject(data, mask, true);
                    fill.EnableOptimization = enableBetaOptimization;
                    fill.material = data.FillMaterial;
                    fill.SetRefrenceIndex(refrenceIndex);
                    fill.SetLines(list);
                    fill.SetViewRect(viewRect, uv);
                    var zero = (float)(-min.y / (max.y - min.y) * viewRect.height);
                    if (negativeFill == false)
                        zero = 0f;
                    fill.MakeFillRender(viewRect, zero, data.StetchFill, true);
                    categoryObj.mFill = fill;
                }

                var catName = data.Name;
                if (data.LineMaterial != null)
                {
                    var lines = CreateDataObject(data, mask, true);

                    var tiling = 1f;
                    if (data.LineTiling.EnableTiling && data.LineTiling.TileFactor > 0f)
                    {
                        var length = 0f;
                        for (var i = 1; i < mTransformed.Count; i++)
                            length += (mTransformed[i - 1] - mTransformed[i]).magnitude;
                        tiling = length / data.LineTiling.TileFactor;
                    }

                    if (tiling <= 0.0001f)
                        tiling = 1f;
                    lines.SetViewRect(viewRect, uv);
                    lines.EnableOptimization = enableBetaOptimization;
                    lines.Thickness = (float)data.LineThickness;
                    lines.Tiling = tiling;
                    lines.SetRefrenceIndex(refrenceIndex);
                    lines.material = data.LineMaterial;
                    lines.SetHoverPrefab(data.LineHoverPrefab);
                    lines.SetLines(list);
                    categoryObj.mLines = lines;
                    lines.Hover += (idx, t, d, pos) => { Lines_Hover(catName, idx, pos); };
                    lines.Click += (idx, t, d, pos) => { Lines_Clicked(catName, idx, pos); };
                    lines.Leave += () => { Lines_Leave(catName); };
                }

                //if (data.PointMaterial != null)
                //{
                var dots = CreateDataObject(data, mask, false);
                if (data.MaskPoints == false)
                    dots.transform.SetParent(transform, true);
                //else
                //{
                //    dots.transform.SetParent(transform, true);
                //    var rectTransform = dots.GetComponent<RectTransform>();
                //    rectTransform.anchorMin = new Vector2(0f, 0f);
                //    rectTransform.anchorMax = new Vector2(0f, 0f);
                //    rectTransform.pivot = new Vector2(0f, 1f);
                //    rectTransform.sizeDelta = viewRect.size;
                //    rectTransform.anchoredPosition = new Vector2(0f, 0f);// viewRect.size.y);
                //}
                categoryObj.mDots = dots;
                dots.material = data.PointMaterial;
                dots.EnableOptimization = enableBetaOptimization;
                dots.SetLines(list);
                var pickRect = viewRect;
                var halfSize = (float)data.PointSize * 0.5f;
                pickRect.xMin -= halfSize;
                pickRect.yMin -= halfSize;
                pickRect.xMax += halfSize;
                pickRect.yMax += halfSize;
                dots.SetViewRect(pickRect, uv);
                if (data.MaskPoints)
                    dots.ClipRect = null;
                else
                    dots.ClipRect = viewRect;
                dots.SetRefrenceIndex(refrenceIndex);
                dots.SetHoverPrefab(data.PointHoverPrefab);

                if (data.PointMaterial != null)
                    dots.MakePointRender((float)data.PointSize);
                else
                    dots.MakePointRender(0f);

                if (mItemLabels != null && mItemLabels.isActiveAndEnabled)
                {
                    var m = new CanvasChartMesh(true);
                    m.RecycleText = true;
                    categoryObj.mItemLabels = m;
                    var textRect = viewRect;
                    textRect.xMin -= 1f;
                    textRect.yMin -= 1f;
                    textRect.xMax += 1f;
                    textRect.yMax += 1f;
                    for (var i = 0; i < mTransformed.Count; i++)
                    {
                        if (mTransformed[i].w == 0f)
                            continue;
                        var pointValue = new DoubleVector2(mTransformed[i]);
                        if (textRect.Contains(pointValue.ToVector2()) == false)
                            continue;
                        if (edit == false)
                            pointValue = Data.GetPoint(data.Name, i + refrenceIndex).ToDoubleVector2();
                        var xFormat = StringFromAxisFormat(pointValue.ToDoubleVector3(), mHorizontalAxis,
                            mItemLabels.FractionDigits, true);
                        var yFormat = StringFromAxisFormat(pointValue.ToDoubleVector3(), mVerticalAxis,
                            mItemLabels.FractionDigits, false);
                        var labelPos = (Vector3)mTransformed[i] + new Vector3(mItemLabels.Location.Breadth,
                            mItemLabels.Seperation, mItemLabels.Location.Depth);
                        if (mItemLabels.Alignment == ChartLabelAlignment.Base)
                            labelPos.y -= mTransformed[i].y;
                        labelPos -= new Vector3(CanvasFitOffset.x * TotalWidth, CanvasFitOffset.y * TotalHeight, 0f);
                        FormatItem(mRealtimeStringBuilder, xFormat, yFormat);
                        var formatted = mRealtimeStringBuilder.ToString();
                        var toSet = mItemLabels.TextFormat.Format(formatted, data.Name, "");
                        var billboard = m.AddText(this, mItemLabels.TextPrefab, transform, mItemLabels.FontSize,
                            mItemLabels.FontSharpness, toSet, labelPos.x, labelPos.y, labelPos.z, 0f, null);
                        //                          BillboardText billboard = ChartCommon.CreateBillboardText(null,mItemLabels.TextPrefab, transform, toSet, labelPos.x, labelPos.y, labelPos.z, 0f, null, hideHierarchy, mItemLabels.FontSize, mItemLabels.FontSharpness);
                        TextController.AddText(billboard);
                        AddBillboardText(data.Name, i + refrenceIndex, billboard);
                    }
                }

                dots.Hover += (idx, t, d, pos) => { Dots_Hover(catName, idx, pos); };
                dots.Click += (idx, t, d, pos) => { Dots_Click(catName, idx, pos); };
                dots.Leave += () => { Dots_Leave(catName); };
                mCategoryObjects[catName] = categoryObj;
            }
        }

        private void Dots_Leave(string category)
        {
            TriggerActiveTextsOut();
            OnItemLeave(new GraphEventArgs(0, Vector3.zero, new DoubleVector2(0.0, 0.0), -1f, category, "", ""),
                "point");
        }

        private void Lines_Leave(string category)
        {
            OnItemLeave(new GraphEventArgs(0, Vector3.zero, new DoubleVector2(0.0, 0.0), -1f, category, "", ""),
                "line");
        }

        private void Dots_Click(string category, int idx, Vector2 pos)
        {
            var point = Data.GetPoint(category, idx);
            Dictionary<int, BillboardText> catgoryTexts;
            BillboardText b;

            if (mTexts.TryGetValue(category, out catgoryTexts))
                if (catgoryTexts.TryGetValue(idx, out b))
                    SelectActiveText(b);
            var xString = StringFromAxisFormat(point, mHorizontalAxis, true);
            var yString = StringFromAxisFormat(point, mVerticalAxis, false);
            OnItemSelected(new GraphEventArgs(idx, pos, point.ToDoubleVector2(), (float)point.z, category, xString,
                yString));
        }

        private void Lines_Clicked(string category, int idx, Vector2 pos)
        {
            var point = Data.GetPoint(category, idx);
            var xString = StringFromAxisFormat(point, mHorizontalAxis, true);
            var yString = StringFromAxisFormat(point, mVerticalAxis, false);
            OnLineSelected(new GraphEventArgs(idx, pos, point.ToDoubleVector2(), (float)point.z, category, xString,
                yString));
        }

        private void Lines_Hover(string category, int idx, Vector2 pos)
        {
            var point = Data.GetPoint(category, idx);
            var xString = StringFromAxisFormat(point, mHorizontalAxis, true);
            var yString = StringFromAxisFormat(point, mVerticalAxis, false);
            OnLineHovered(new GraphEventArgs(idx, pos, point.ToDoubleVector2(), (float)point.z, category, xString,
                yString));
        }

        private void Dots_Hover(string category, int idx, Vector2 pos)
        {
            var point = Data.GetPoint(category, idx);
            Dictionary<int, BillboardText> catgoryTexts;
            BillboardText b;

            if (mTexts.TryGetValue(category, out catgoryTexts))
                if (catgoryTexts.TryGetValue(idx, out b))
                    SelectActiveText(b);

            var xString = StringFromAxisFormat(point, mHorizontalAxis, true);
            var yString = StringFromAxisFormat(point, mVerticalAxis, false);
            OnItemHoverted(new GraphEventArgs(idx, pos, point.ToDoubleVector2(), (float)point.z, category, xString,
                yString));
        }

        protected override void OnItemHoverted(object userData)
        {
            base.OnItemHoverted(userData);
            var args = userData as GraphEventArgs;
            AddOccupiedCategory(args.Category, "point");
        }

        protected override void OnItemSelected(object userData)
        {
            base.OnItemSelected(userData);
            var args = userData as GraphEventArgs;
            AddOccupiedCategory(args.Category, "point");
        }

        private void AddOccupiedCategory(string cat, string type)
        {
            mOccupiedCateogies.Add(cat + "|" + type);
        }

        protected override void OnItemLeave(object userData, string type)
        {
            var args = userData as GraphEventArgs;
            if (args == null)
                return;

            var item = args.Category + "|" + type;
            mOccupiedCateogies.Remove(item);
            mOccupiedCateogies.RemoveWhere(x => !Data.HasCategory(x.Split('|')[0]));

            if (mOccupiedCateogies.Count == 0)
                if (NonHovered != null)
                    NonHovered.Invoke();
        }

        internal override void SetAsMixedSeries()
        {
            throw new NotImplementedException();
        }

        private class CategoryObject
        {
            public readonly Dictionary<int, (DoubleVector3, string)> mCahced = new();
            public CanvasLines mDots;
            public CanvasLines mFill;
            public CanvasChartMesh mItemLabels;
            public CanvasLines mLines;
        }
    }
}