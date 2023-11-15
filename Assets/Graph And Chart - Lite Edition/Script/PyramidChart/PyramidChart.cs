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
    public class PyramidChart : AnyChart
    {
        public enum JustificationType
        {
            LeftAligned,
            RightAligned,
            CenterAligned
        }

        public enum SlopeType
        {
            Center,
            Left,
            Right,
            Custom
        }

        /// <summary>
        ///     occures when a pie item is clicked
        /// </summary>
        public PyramidEvent ItemClicked = new();

        /// <summary>
        ///     occures when a pie item is hovered
        /// </summary>
        public PyramidEvent ItemHovered = new();

        /// <summary>
        ///     occurs when no pie is hovered any longer
        /// </summary>
        public UnityEvent NonHovered = new();

        [SerializeField] [Tooltip("The material of the back of the pyramid")]
        public Material backMaterial;


        [SerializeField] [Tooltip("The inset of each pyramid component")]
        public float inset;


        [SerializeField] [Tooltip("the text justification of the pyramid chart")]
        private JustificationType justification;


        [SerializeField] [Tooltip("the slope type of the pyramid")]
        public SlopeType slope;


        [SerializeField] [Tooltip("prefab for the pyramid item. must contain a PyramidCanvasGenerator component")]
        private PyramidCanvasGenerator prefab;


        [HideInInspector] [SerializeField] private PyramidData Data = new();

        private Dictionary<string, PyramidObject> mPyramids = new();

        private bool mQuick;


        private float totalWidth, totalHeight;


        /// <summary>
        ///     The inset of each pyramid component
        /// </summary>
        public Material BackMaterial
        {
            get => backMaterial;
            set
            {
                backMaterial = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     The inset of each pyramid component
        /// </summary>
        public float Inset
        {
            get => inset;
            set
            {
                inset = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     the text justification of the pyramid chart
        /// </summary>
        public JustificationType Justification
        {
            get => justification;
            set
            {
                justification = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     prefab for the pie item. must contain a PieCanvasGenerator component
        /// </summary>
        public SlopeType Slope
        {
            get => slope;
            set
            {
                slope = value;
                OnPropertyUpdated();
            }
        }


        /// <summary>
        ///     prefab for the pie item. must contain a PieCanvasGenerator component
        /// </summary>
        public PyramidCanvasGenerator Prefab
        {
            get => prefab;
            set
            {
                prefab = value;
                OnPropertyUpdated();
            }
        }

        protected override IChartData DataLink => Data;

        public PyramidData DataSource => Data;

        public override bool SupportRealtimeGeneration => false;

        protected override LegenedData LegendInfo
        {
            get
            {
                var legend = new LegenedData();
                if (Data == null)
                    return legend;
                foreach (var column in ((IInternalPyramidData)Data).InternalDataSource.Columns)
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

        protected override bool SupportsItemLabels => false;

        protected override bool ShouldFitCanvas => false;

        protected override float TotalDepthLink => 0f;

        protected override float TotalHeightLink => totalHeight;

        protected override float TotalWidthLink => totalWidth;

        public override bool IsCanvas => true;

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
            if (Application.isPlaying) HookEvents();
            Invalidate();
        }

        private void HookEvents()
        {
            Data.ProperyUpdated -= Data_ProperyUpdated;
            Data.ProperyUpdated += Data_ProperyUpdated;
            Data.RealtimeProperyUpdated -= Data_RealtimeProperyUpdated;
            Data.RealtimeProperyUpdated += Data_RealtimeProperyUpdated;

            ((IInternalPyramidData)Data).InternalDataSource.DataStructureChanged -= MDataSource_DataStructureChanged;
            ((IInternalPyramidData)Data).InternalDataSource.DataStructureChanged += MDataSource_DataStructureChanged;
            ((IInternalPyramidData)Data).InternalDataSource.DataValueChanged -= MDataSource_DataValueChanged;
            ((IInternalPyramidData)Data).InternalDataSource.DataValueChanged += MDataSource_DataValueChanged;
        }

        private void Data_RealtimeProperyUpdated()
        {
            QuickInvalidate();
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
        }

        private void MDataSource_DataStructureChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void ClearChart()
        {
            base.ClearChart();
            mPyramids.Clear();
        }

        private Vector3 AlignTextPosition(AlignedItemLabels labels, PyramidObject obj, IPyramidGenerator generator,
            float height)
        {
            var position = new Vector3(labels.Location.Breadth, labels.Seperation, labels.Location.Depth);
            position.y += height;
            position = generator.GetTextPosition(justification, labels.Alignment == ChartLabelAlignment.Base);
            return position;
        }

        protected IPyramidGenerator PreparePyramidObject(out GameObject pyramidObject)
        {
            if (Prefab == null)
                pyramidObject = new GameObject();
            else
                pyramidObject = Instantiate(Prefab.gameObject);
            ChartCommon.EnsureComponent<RectTransform>(pyramidObject);
            ChartCommon.EnsureComponent<CanvasRenderer>(pyramidObject);
            return ChartCommon.EnsureComponent<PyramidCanvasGenerator>(pyramidObject);
        }

        private void GeneratePyramid(bool update)
        {
            if (update == false)
                ClearChart();
            else
                EnsureTextController();
            if (((IInternalPyramidData)Data).InternalDataSource == null)
                return;

            var data = ((IInternalPyramidData)Data).InternalDataSource.getRawData();
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

            var rectTrans = GetComponent<RectTransform>();
            totalHeight = rectTrans.rect.height;
            totalWidth = rectTrans.rect.width;

            float baseX1 = 0;
            var baseX2 = totalWidth;
            float accumilatedHeight = 0;
            float? firstCenterHeight = null;
            float acummilatedWeight = 0;
            for (var i = 0; i < columnCount; ++i)
            {
                var userData = ((IInternalPyramidData)Data).InternalDataSource.Columns[i].UserData;
                var categoryData = (PyramidData.CategoryData)userData;

                var name = ((IInternalPyramidData)Data).InternalDataSource.Columns[i].Name;
                var amount = Math.Max(data[0, i], 0);
                if (amount == 0f)
                    continue;

                var weight = (float)(amount / total);
                var actualHeight = totalHeight * weight;

                var slopeRight = categoryData.RightSlope;
                var slopeLeft = categoryData.LeftSlope;
                float atan;
                switch (slope)
                {
                    case SlopeType.Center:
                        atan = -Mathf.Atan2(totalHeight, totalWidth * 0.5f) * Mathf.Rad2Deg + 90;
                        slopeRight = atan;
                        slopeLeft = atan;
                        break;
                    case SlopeType.Left:
                        atan = -Mathf.Atan2(totalHeight, totalWidth) * Mathf.Rad2Deg + 90;
                        slopeLeft = 0;
                        slopeRight = atan;
                        break;
                    case SlopeType.Right:
                        atan = -Mathf.Atan2(totalHeight, totalWidth) * Mathf.Rad2Deg + 90;
                        slopeLeft = atan;
                        slopeRight = 0;
                        break;
                }

                GameObject pyramidObject = null;
                GameObject pyramidBackObject = null;
                IPyramidGenerator generator = null;
                IPyramidGenerator backgenerator = null;
                PyramidObject dataObject;
                var centerHeight = actualHeight * 0.5f + accumilatedHeight;
                var unblendedHeight = centerHeight;
                if (firstCenterHeight.HasValue == false)
                    firstCenterHeight = centerHeight;
                centerHeight = Mathf.Lerp(firstCenterHeight.Value, centerHeight, categoryData.PositionBlend);

                if (mPyramids.TryGetValue(name, out dataObject))
                {
                    pyramidBackObject = dataObject.backObject;
                    pyramidObject = dataObject.pyramidObject;
                    backgenerator = dataObject.BackGenerator;
                    generator = dataObject.Generator;
                    generator.SetParams(baseX1, baseX2, totalWidth, slopeLeft, slopeRight, actualHeight, inset, 0f, 1f);
                    if (backgenerator != null)
                        backgenerator.SetParams(baseX1, baseX2, totalWidth, slopeLeft, slopeRight, actualHeight, 0f,
                            acummilatedWeight, acummilatedWeight + weight);
                    if (dataObject.ItemLabel)
                    {
                        var labelPos = AlignTextPosition(mItemLabels, dataObject, generator, centerHeight);
                        dataObject.ItemLabel.transform.localPosition = labelPos;
                        ChartCommon.UpdateTextParams(dataObject.ItemLabel.UIText, categoryData.Title);
                    }
                }
                else
                {
                    dataObject = new PyramidObject();

                    if (backMaterial != null)
                    {
                        var backGenerator = PreparePyramidObject(out pyramidBackObject);
                        backGenerator.SetParams(baseX1, baseX2, totalWidth, slopeLeft, slopeRight, actualHeight, 0f,
                            acummilatedWeight, acummilatedWeight + weight);
                        dataObject.backObject = pyramidBackObject;
                        dataObject.BackGenerator = backGenerator;
                        ChartCommon.HideObject(pyramidBackObject, hideHierarchy);
                        pyramidBackObject.transform.SetParent(transform, false);
                        ChartCommon.EnsureComponent<ChartItem>(pyramidBackObject);
                        var backcontrol = ChartCommon.EnsureComponent<ChartMaterialController>(pyramidBackObject);
                        backcontrol.Materials = new ChartDynamicMaterial(backMaterial);
                        foreach (var itemEffect in pyramidBackObject.GetComponents<ChartItemEffect>())
                            ChartCommon.SafeDestroy(itemEffect);
                        ChartCommon.SafeDestroy(backGenerator.ContainerObject);
                    }

                    generator = PreparePyramidObject(out pyramidObject);
                    generator.SetParams(baseX1, baseX2, totalWidth, slopeLeft, slopeRight, actualHeight, inset, 0f, 1f);
                    ChartCommon.HideObject(pyramidObject, hideHierarchy);
                    pyramidObject.transform.SetParent(transform, false);
                    ChartCommon.EnsureComponent<ChartItem>(pyramidObject);


                    var control = ChartCommon.EnsureComponent<ChartMaterialController>(pyramidObject);
                    control.Materials = Data.GetMaterial(name);
                    control.Refresh();


                    dataObject.Generator = generator;
                    dataObject.category = name;
                    dataObject.pyramidObject = pyramidObject;
                    mPyramids.Add(name, dataObject);

                    var effect = ChartCommon.EnsureComponent<CharItemEffectController>(pyramidObject);
                    effect.WorkOnParent = false;
                    effect.InitialScale = false;

                    var events = pyramidObject.GetComponentsInChildren<ChartItemEvents>();
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
                        var labelPos = AlignTextPosition(mItemLabels, dataObject, generator, 0f);
                        var angle = justification == JustificationType.LeftAligned ? -180f : 180f;
                        var billboard = ChartCommon.CreateBillboardText(null, mItemLabels.TextPrefab,
                            dataObject.pyramidObject.transform, categoryData.Title, labelPos.x, labelPos.y, labelPos.z,
                            angle, null, hideHierarchy, mItemLabels.FontSize, mItemLabels.FontSharpness);
                        dataObject.ItemLabel = billboard;
                        dataObject.ItemLabel.transform.localPosition = labelPos;
                        TextController.AddText(billboard);
                    }
                }

                dataObject.Text = categoryData.Text;
                dataObject.Title = categoryData.Title;

                if (IsCanvas)
                {
                    if (pyramidObject != null)
                    {
                        var actualPosition = new Vector2(0.5f, centerHeight) + categoryData.Shift;
                        actualPosition = new Vector2(actualPosition.x, actualPosition.y / TotalHeight);
                        var objectRect = pyramidObject.GetComponent<RectTransform>();
                        objectRect.pivot = new Vector2(0.5f, 0.5f);
                        objectRect.anchorMin = actualPosition;
                        objectRect.anchorMax = actualPosition;
                        objectRect.anchoredPosition = new Vector2();
                        objectRect.sizeDelta = new Vector2(totalWidth, actualHeight);
                    }

                    if (pyramidBackObject != null)
                    {
                        var actualPosition = new Vector2(0.5f, unblendedHeight);
                        actualPosition = new Vector2(actualPosition.x, actualPosition.y / TotalHeight);
                        var objectRect = pyramidBackObject.GetComponent<RectTransform>();
                        objectRect.pivot = new Vector2(0f, 0f);
                        objectRect.anchorMin = actualPosition;
                        objectRect.anchorMax = actualPosition;
                        objectRect.anchoredPosition = new Vector2();
                    }
                }

                accumilatedHeight += actualHeight;
                acummilatedWeight += weight;
                if (backgenerator != null)
                    backgenerator.Generate();
                generator.Generate();
                generator.GetUpperBase(out baseX1, out baseX2);
                generator.ApplyInfo(categoryData.Title, categoryData.Text, categoryData.Image, categoryData.Scale);
                generator.SetAlpha(categoryData.Alpha);
            }
        }

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
            GeneratePyramid(mQuick);
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
            if (inset < 0)
                inset = 0;
        }

        private PyramidEventArgs userDataToEventArgs(object userData)
        {
            var pyramid = (PyramidObject)userData;
            return new PyramidEventArgs(pyramid.category, pyramid.Title, pyramid.Text);
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
            if (ItemHovered != null)
                ItemHovered.Invoke(userDataToEventArgs(userData));
        }

        protected override void OnItemSelected(object userData)
        {
            base.OnItemSelected(userData);
            var args = userDataToEventArgs(userData);
            if (ItemClicked != null)
                ItemClicked.Invoke(args);
        }

        public class PyramidEventArgs
        {
            public PyramidEventArgs(string category, string title, string text)
            {
                Title = title;
                Text = text;
                Category = category;
            }

            public string Category { get; }
            public string Title { get; }
            public string Text { get; }
        }

        [Serializable]
        public class PyramidEvent : UnityEvent<PyramidEventArgs>
        {
        }

        public class PyramidObject
        {
            public IPyramidGenerator BackGenerator;
            public GameObject backObject;
            public string category;
            public IPyramidGenerator Generator;
            public BillboardText ItemLabel;
            public GameObject pyramidObject;
            public string Title, Text;
        }
    }
}