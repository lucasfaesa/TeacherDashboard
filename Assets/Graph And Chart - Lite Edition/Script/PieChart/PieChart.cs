#define Graph_And_Chart_PRO

using System;
using System.Collections.Generic;
using ChartAndGraph.DataSource;
using UnityEngine;
using UnityEngine.Events;

namespace ChartAndGraph
{
    /// <summary>
    ///     Pie chart class
    /// </summary>
    [ExecuteInEditMode]
    [Serializable]
    public abstract class PieChart : AnyChart
    {
        /// <summary>
        ///     occures when a pie item is clicked
        /// </summary>
        public PieEvent PieClicked = new();

        /// <summary>
        ///     occures when a pie item is hovered
        /// </summary>
        public PieEvent PieHovered = new();

        /// <summary>
        ///     occurs when no pie is hovered any longer
        /// </summary>
        public UnityEvent NonHovered = new();

        [SerializeField] [Tooltip("The number of mesh segements in each pie slice")]
        private int meshSegements = 20;

        [SerializeField] [Tooltip("The start angle of the pie chart")]
        private float startAngle;

        [SerializeField] [Range(0f, 360f)] [Tooltip("The angle span of the pie chart")]
        private float angleSpan = 360;

        [SerializeField] [Range(0f, 360f)] [Tooltip("The spacing angle of the pie chart")]
        private float spacingAngle;

        [SerializeField] [Tooltip("The outer radius of the pie chart")]
        protected float radius;

        [SerializeField] [Tooltip("The inner radius of the pie chart")]
        private float torusRadius;

        [SerializeField] [Tooltip("The extrusion of each pie slice")]
        private float extrusion;


        [HideInInspector] [SerializeField] private PieData Data = new();

        [SerializeField] [Tooltip("draw the pie in a clockwise order ")]
        private bool clockWise;


        private GameObject mFixPositionPie;

        /// <summary>
        ///     the bars generated for the chart
        /// </summary>
        [NonSerialized] private Dictionary<string, PieObject> mPies = new();

        private bool mQuick;

        public bool ClockWise
        {
            get => clockWise;
            set
            {
                clockWise = value;
                Invalidate();
            }
        }

        protected override IChartData DataLink => Data;

        public PieData DataSource => Data;

        protected abstract float LineSpacingLink { get; }

        protected abstract float LineThicknessLink { get; }

        protected abstract Material LineMaterialLink { get; }

        protected override float TotalDepthLink => 0.0f;

        protected override float TotalHeightLink => (radius + extrusion) * 2f;

        protected override float TotalWidthLink => (radius + extrusion) * 2f;

        /// <summary>
        ///     The number of mesh segements in each pie slice
        /// </summary>
        public int MeshSegements
        {
            get => meshSegements;
            set
            {
                meshSegements = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     The angle span of the pie chart
        /// </summary>
        public float AngleSpan
        {
            get => angleSpan;
            set
            {
                angleSpan = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     The spacing angle of the pie chart
        /// </summary>
        public float SpacingAngle
        {
            get => spacingAngle;
            set
            {
                spacingAngle = value;
                OnPropertyUpdated();
            }
        }

        public override bool SupportRealtimeGeneration => false;

        /// <summary>
        ///     The outer radius of the pie chart
        /// </summary>
        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     The inner radius of the pie chart
        /// </summary>
        public float TorusRadius
        {
            get => torusRadius;
            set
            {
                torusRadius = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     The start angle of the pie chart
        /// </summary>
        public float StartAngle
        {
            get => startAngle;
            set
            {
                startAngle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     The extrusion of each pie slice
        /// </summary>
        public float Extrusion
        {
            get => extrusion;
            set
            {
                extrusion = value;
                OnPropertyUpdated();
            }
        }

        protected abstract float InnerDepthLink { get; }

        protected abstract float OuterDepthLink { get; }

        protected override LegenedData LegendInfo
        {
            get
            {
                var legend = new LegenedData();
                if (Data == null)
                    return legend;
                foreach (var column in ((IInternalPieData)Data).InternalDataSource.Columns)
                {
                    var item = new LegenedData.LegenedItem();
                    item.Name = column.Name;
                    if (column.Material != null)
                        item.Material = column.Material.Normal;
                    else
                        item.Material = null;
                    legend.AddLegenedItem(item);
                }

                return legend;
            }
        }

        protected override bool SupportsCategoryLabels => true;

        protected override bool SupportsGroupLables => false;

        protected override bool SupportsItemLabels => true;

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

        public void OnDidApplyAnimationProperties()
        {
            OnPropertyUpdated();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying) HookEvents();
            Invalidate();
        }

        private void HookEvents()
        {
            Data.ProperyUpdated -= Data_ProperyUpdated;
            Data.ProperyUpdated += Data_ProperyUpdated;
            ((IInternalPieData)Data).InternalDataSource.DataStructureChanged -= MDataSource_DataStructureChanged;
            ((IInternalPieData)Data).InternalDataSource.DataStructureChanged += MDataSource_DataStructureChanged;
            ((IInternalPieData)Data).InternalDataSource.DataValueChanged -= MDataSource_DataValueChanged;
            ((IInternalPieData)Data).InternalDataSource.DataValueChanged += MDataSource_DataValueChanged;
        }

        private void Data_ProperyUpdated()
        {
            Invalidate();
        }

        protected void QuickInvalidate()
        {
            if (Invalidating)
                return;
            Invalidate();
            mQuick = true;
        }

        public override void Invalidate()
        {
            base.Invalidate();
            mQuick = false;
        }

        private void MDataSource_DataValueChanged(object sender, ChartDataSourceBase.DataValueChangedEventArgs e)
        {
            QuickInvalidate();
//            GeneratePie(true);
        }

        private void MDataSource_DataStructureChanged(object sender, EventArgs e)
        {
            Invalidate();
//            GenerateChart();
        }

        protected override void ClearChart()
        {
            base.ClearChart();
            mPies.Clear();
            mFixPositionPie = null;
        }

        private Vector3 AlignTextPosition(AlignedItemLabels labels, PieObject obj, out CanvasLines.LineSegement line,
            float modifiedRaidus)
        {
            line = null;
            var angle = obj.StartAngle + obj.AngleSpan * 0.5f;
            var position = new Vector3(labels.Seperation, labels.Location.Breadth, labels.Location.Depth);
            position = Quaternion.AngleAxis(angle, Vector3.forward) * position;
            var alignRadius = (modifiedRaidus + TorusRadius) * 0.5f;
            Vector3 atAngle = ChartCommon.FromPolar(angle, 1f);
            if (labels.Alignment == ChartLabelAlignment.Top)
            {
                alignRadius = Mathf.Max(modifiedRaidus, TorusRadius);
                var basePosition = atAngle * alignRadius;
                var end = basePosition + position;
                end -= position.normalized * LineSpacingLink;
                Vector4[] arr = { basePosition, end };
                arr[0].w = -1f;
                arr[1].w = -1f;
                line = new CanvasLines.LineSegement(arr);
            }

            position += atAngle * alignRadius;
            return position;
        }

        private CanvasLines AddLineRenderer(GameObject topObject, CanvasLines.LineSegement line)
        {
            var obj = ChartCommon.CreateCanvasChartItem();
            obj.transform.SetParent(topObject.transform);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = new Vector3(0f, 0f, 0f);
            obj.transform.localRotation = Quaternion.identity;
            ChartCommon.EnsureComponent<CanvasRenderer>(obj);
            var lines = obj.AddComponent<CanvasLines>();
            lines.raycastTarget = false;
            var lst = new List<CanvasLines.LineSegement>();
            lst.Add(line);
            lines.SetLines(lst);
            lines.Thickness = LineThicknessLink;
            lines.material = LineMaterialLink;
            return lines;
        }

        private void GeneratePie(bool update)
        {
            if (mFixPositionPie == null)
                update = false;
            if (update == false)
                ClearChart();
            else
                EnsureTextController();
            if (((IInternalPieData)Data).InternalDataSource == null)
                return;

            var data = ((IInternalPieData)Data).InternalDataSource.getRawData();
            var rowCount = data.GetLength(0);
            var columnCount = data.GetLength(1);

            if (rowCount != 1) // row count for pie must be 1
                return;

            var total = 0.0;

            for (var i = 0; i < columnCount; ++i)
            {
                var val = Math.Max(data[0, i], 0);
                total += val;
            }

            var start = startAngle;
            if (clockWise)
                start -= angleSpan;
            var totalGaps = columnCount * spacingAngle;
            var spanWithoutGaps = angleSpan - totalGaps;

            if (spanWithoutGaps < 0f)
                spanWithoutGaps = 0f;

            if (mFixPositionPie == null)
            {
                mFixPositionPie = new GameObject("FixPositionPie", typeof(ChartItem));
                ChartCommon.HideObject(mFixPositionPie, hideHierarchy);
                mFixPositionPie.transform.SetParent(transform, false);
                if (IsCanvas)
                {
                    var rectTrans = mFixPositionPie.AddComponent<RectTransform>();
                    rectTrans.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTrans.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTrans.pivot = new Vector2(0.5f, 0.5f);
                    rectTrans.anchoredPosition = new Vector2(0.5f, 0.5f);
                }
            }

            for (var i = 0; i < columnCount; ++i)
            {
                var userData = ((IInternalPieData)Data).InternalDataSource.Columns[i].UserData;
                var radiusScale = 1f;
                var depthScale = 1f;
                var depthOffset = 0f;
                if (userData != null && userData is PieData.CategoryData)
                {
                    radiusScale = ((PieData.CategoryData)userData).RadiusScale;
                    depthScale = ((PieData.CategoryData)userData).DepthScale;
                    depthOffset = ((PieData.CategoryData)userData).DepthOffset;
                }

                if (radiusScale <= 0.001f)
                    radiusScale = 1f;
                if (depthScale <= 0.001f)
                    depthScale = 1f;
                var name = ((IInternalPieData)Data).InternalDataSource.Columns[i].Name;
                var amount = Math.Max(data[0, i], 0);
                if (amount == 0f)
                    continue;
                var weight = (float)(amount / total);
                var currentSpan = spanWithoutGaps * weight;
                GameObject pieObject = null;
                IPieGenerator generator = null;
                PieObject dataObject;
                CanvasLines.LineSegement line;
                var modifiedRadius = Mathf.Max(radius * radiusScale, torusRadius);
                //  float modifiedDepth = d
                var lineAngle = start + currentSpan * 0.5f;
                if (mPies.TryGetValue(name, out dataObject))
                {
                    dataObject.StartAngle = start;
                    dataObject.AngleSpan = currentSpan;
                    generator = dataObject.Generator;
                    if (dataObject.ItemLabel)
                    {
                        var labelPos = AlignTextPosition(mItemLabels, dataObject, out line, modifiedRadius);
                        dataObject.ItemLabel.transform.localPosition = labelPos;
                        var toSet = ChartAdancedSettings.Instance.FormatFractionDigits(mItemLabels.FractionDigits,
                            amount, CustomNumberFormat);
                        toSet = mItemLabels.TextFormat.Format(toSet, name, "");
                        ChartCommon.UpdateTextParams(dataObject.ItemLabel.UIText, toSet);
                        if (dataObject.ItemLine != null)
                        {
                            var lst = new List<CanvasLines.LineSegement>();
                            lst.Add(line);
                            dataObject.ItemLine.SetLines(lst);
                        }
                    }

                    if (dataObject.CategoryLabel != null)
                    {
                        var labelPos = AlignTextPosition(mCategoryLabels, dataObject, out line, modifiedRadius);
                        dataObject.CategoryLabel.transform.localPosition = labelPos;
                        if (dataObject.CategoryLine != null)
                        {
                            var lst = new List<CanvasLines.LineSegement>();
                            lst.Add(line);
                            dataObject.CategoryLine.SetLines(lst);
                        }
                    }

                    var add = ChartCommon.FromPolar(start + currentSpan * 0.5f, Extrusion);
                    dataObject.TopObject.transform.localPosition = new Vector3(add.x, add.y, 0f);
                }
                else
                {
                    var topObject = new GameObject();
                    if (IsCanvas)
                        topObject.AddComponent<RectTransform>();
                    ChartCommon.HideObject(topObject, hideHierarchy);
                    topObject.AddComponent<ChartItem>();
                    topObject.transform.SetParent(mFixPositionPie.transform);
                    topObject.transform.localPosition = new Vector3();
                    topObject.transform.localRotation = Quaternion.identity;
                    topObject.transform.localScale = new Vector3(1f, 1f, 1f);

                    generator = PreparePieObject(out pieObject);

                    ChartCommon.EnsureComponent<ChartItem>(pieObject);
                    var control = ChartCommon.EnsureComponent<ChartMaterialController>(pieObject);
                    control.Materials = Data.GetMaterial(name);
                    control.Refresh();
                    dataObject = new PieObject();
                    dataObject.StartAngle = start;
                    dataObject.AngleSpan = currentSpan;
                    dataObject.TopObject = topObject;
                    dataObject.Generator = generator;
                    dataObject.category = name;
                    var pieInfo = pieObject.AddComponent<PieInfo>();
                    pieInfo.pieObject = dataObject;
                    pieObject.transform.SetParent(topObject.transform);
                    var add = ChartCommon.FromPolar(start + currentSpan * 0.5f, Extrusion);
                    pieObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                    pieObject.transform.localScale = new Vector3(1f, 1f, 1f);
                    pieObject.transform.localRotation = Quaternion.identity;
                    mPies.Add(name, dataObject);

                    topObject.transform.localPosition = new Vector3(add.x, add.y, 0f);
                    var effect = ChartCommon.EnsureComponent<CharItemEffectController>(pieObject);
                    effect.WorkOnParent = true;
                    effect.InitialScale = false;

                    var events = pieObject.GetComponentsInChildren<ChartItemEvents>();

                    for (var j = 0; j < events.Length; ++j)
                    {
                        if (events[j] == null)
                            continue;
                        InternalItemEvents comp = events[j];
                        comp.Parent = this;
                        comp.UserData = dataObject;
                    }

                    if (mItemLabels != null)
                    {
                        var labelPos = AlignTextPosition(mItemLabels, dataObject, out line, modifiedRadius);
                        if (line != null && IsUnderCanvas)
                            dataObject.ItemLine = AddLineRenderer(topObject, line);
                        var toSet = ChartAdancedSettings.Instance.FormatFractionDigits(mItemLabels.FractionDigits,
                            amount, CustomNumberFormat);
                        toSet = mItemLabels.TextFormat.Format(toSet, name, "");
                        var billboard = ChartCommon.CreateBillboardText(null, mItemLabels.TextPrefab,
                            topObject.transform, toSet, labelPos.x, labelPos.y, labelPos.z, lineAngle,
                            topObject.transform, hideHierarchy, mItemLabels.FontSize, mItemLabels.FontSharpness);
                        dataObject.ItemLabel = billboard;
                        TextController.AddText(billboard);
                    }

                    if (mCategoryLabels != null)
                    {
                        var labelPos = AlignTextPosition(mCategoryLabels, dataObject, out line, modifiedRadius);
                        if (line != null && IsUnderCanvas)
                            dataObject.CategoryLine = AddLineRenderer(topObject, line);
                        var toSet = name;
                        toSet = mCategoryLabels.TextFormat.Format(toSet, "", "");
                        var billboard = ChartCommon.CreateBillboardText(null, mCategoryLabels.TextPrefab,
                            topObject.transform, toSet, labelPos.x, labelPos.y, labelPos.z, lineAngle,
                            topObject.transform, hideHierarchy, mCategoryLabels.FontSize,
                            mCategoryLabels.FontSharpness);
                        dataObject.CategoryLabel = billboard;
                        TextController.AddText(billboard);
                    }
                }

                var maxDepth = Mathf.Max(OuterDepthLink, InnerDepthLink);
                var depthSize = maxDepth * depthScale;
                if (pieObject != null)
                {
                    var depthStart = (maxDepth - depthSize) * 0.5f;
                    pieObject.transform.localPosition = new Vector3(0f, 0f, depthStart - depthSize * depthOffset);
                }

                dataObject.Value = (float)data[0, i];
                generator.Generate(Mathf.Deg2Rad * start, Mathf.Deg2Rad * currentSpan, modifiedRadius, torusRadius,
                    meshSegements, OuterDepthLink * depthScale, InnerDepthLink * depthScale);
                start += spacingAngle + currentSpan;
            }
        }

        protected abstract IPieGenerator PreparePieObject(out GameObject pieObject);


        protected override void OnLabelSettingChanged()
        {
            base.OnLabelSettingChanged();
            Invalidate();
        }

        protected override void OnLabelSettingsSet()
        {
            base.OnLabelSettingsSet();
            Invalidate();
        }

        public override void InternalGenerateChart()
        {
            if (gameObject.activeInHierarchy == false)
                return;
            base.InternalGenerateChart();
            GeneratePie(mQuick);
            mQuick = false;
        }

        protected override bool HasValues(AxisBase axis)
        {
            return false;
        }

        protected override double MaxValue(AxisBase axis)
        {
            return 0.0;
        }

        protected override double MinValue(AxisBase axis)
        {
            return 0.0;
        }

        protected virtual void OnPropertyChanged()
        {
            QuickInvalidate();
        }

        protected override void OnPropertyUpdated()
        {
            base.OnPropertyUpdated();
            Invalidate();
        }

        protected override void ValidateProperties()
        {
            base.ValidateProperties();
            if (extrusion < 0)
                extrusion = 0f;
            if (radius < 0f)
                radius = 0;
            if (torusRadius < 0f)
                torusRadius = 0f;
            if (torusRadius > radius)
                torusRadius = radius;
            if (angleSpan < 10f)
                angleSpan = 10f;
            if (spacingAngle < 0f)
                spacingAngle = 0f;
        }

        private PieEventArgs userDataToEventArgs(object userData)
        {
            var pie = (PieObject)userData;
            return new PieEventArgs(pie.category, pie.Value);
        }

        protected override void OnNonHoverted()
        {
            base.OnNonHoverted();
            if (NonHovered != null)
                NonHovered.Invoke();
        }

        protected override void OnItemHoverted(object userData)
        {
            base.OnItemHoverted(userData);
            if (PieHovered != null)
                PieHovered.Invoke(userDataToEventArgs(userData));
        }

        protected override void OnItemSelected(object userData)
        {
            base.OnItemSelected(userData);
            var args = userDataToEventArgs(userData);
            if (PieClicked != null)
                PieClicked.Invoke(args);
        }

        public class PieEventArgs
        {
            public PieEventArgs(string category, double value)
            {
                Value = value;
                Category = category;
            }

            public double Value { get; }
            public string Category { get; }
        }

        [Serializable]
        public class PieEvent : UnityEvent<PieEventArgs>
        {
        }

        public class PieObject
        {
            public float AngleSpan;
            public string category;
            public BillboardText CategoryLabel;
            public CanvasLines CategoryLine;
            public IPieGenerator Generator;
            public BillboardText ItemLabel;
            public CanvasLines ItemLine;
            public float StartAngle;
            public GameObject TopObject;
            public float Value;
        }
    }
}