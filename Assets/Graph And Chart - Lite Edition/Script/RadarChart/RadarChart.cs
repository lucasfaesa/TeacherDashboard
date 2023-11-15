using System;
using System.Collections.Generic;
using ChartAndGraph.DataSource;
using UnityEngine;
using UnityEngine.Events;

namespace ChartAndGraph
{
    [ExecuteInEditMode]
    public abstract class RadarChart : AnyChart
    {
        [SerializeField] private float radius = 3f;

        [SerializeField] private float angle;

        [SerializeField] private Material axisPointMaterial;


        [SerializeField] private Material axisLineMaterial;

        [SerializeField] private float axisThickness = 0.1f;

        [SerializeField] private float axisPointSize = 1f;

        [SerializeField] private float axisAdd;

        [SerializeField] private int totalAxisDevisions = 5;

        /// <summary>
        ///     occures when a point is clicked
        /// </summary>
        public RadarEvent PointClicked = new();

        /// <summary>
        ///     occurs when a point is hovered
        /// </summary>
        public RadarEvent PointHovered = new();

        /// <summary>
        ///     occurs when no point is hovered any longer
        /// </summary>
        public UnityEvent NonHovered = new();

        /// <summary>
        ///     the radar data
        /// </summary>
        [HideInInspector] [SerializeField] private RadarChartData Data = new();

        protected HashSet<BillboardText> mActiveTexts = new();

        private readonly List<GameObject> mAxisObjects = new();
        private Vector3[] mDirections;
        private readonly HashSet<string> mOccupiedCateogies = new();
        protected Dictionary<string, List<BillboardText>> mTexts = new();

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                GenerateChart();
            }
        }

        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                GenerateChart();
            }
        }

        public Material AxisPointMaterial
        {
            get => axisPointMaterial;
            set
            {
                axisPointMaterial = value;
                GenerateChart();
            }
        }

        public Material AxisLineMaterial
        {
            get => axisLineMaterial;
            set
            {
                axisLineMaterial = value;
                GenerateChart();
            }
        }

        public float AxisThickness
        {
            get => axisThickness;
            set
            {
                axisThickness = value;
                GenerateChart();
            }
        }

        public float AxisPointSize
        {
            get => axisPointSize;
            set
            {
                axisPointSize = value;
                GenerateChart();
            }
        }

        public float AxisAdd
        {
            get => axisAdd;
            set
            {
                axisAdd = value;
                GenerateChart();
            }
        }

        public int TotalAxisDevisions
        {
            get => totalAxisDevisions;
            set
            {
                totalAxisDevisions = value;
                GenerateChart();
            }
        }

        /// <summary>
        ///     Holds the radar chart data. including values, categories and groups.
        /// </summary>
        public RadarChartData DataSource => Data;

        public override bool SupportRealtimeGeneration => false;

        protected override LegenedData LegendInfo
        {
            get
            {
                var legend = new LegenedData();
                if (Data == null)
                    return legend;
                foreach (var column in ((IInternalRadarData)Data).InternalDataSource.Columns)
                {
                    var item = new LegenedData.LegenedItem();
                    var catData = column.UserData as RadarChartData.CategoryData;
                    item.Name = column.Name;
                    if (catData.FillMaterial != null)
                    {
                        item.Material = catData.FillMaterial;
                    }
                    else
                    {
                        if (catData.LineMaterial != null)
                            item.Material = catData.LineMaterial;
                        else
                            item.Material = null;
                    }

                    legend.AddLegenedItem(item);
                }

                return legend;
            }
        }

        protected override IChartData DataLink => Data;

        protected override bool SupportsCategoryLabels => true;

        protected override bool SupportsGroupLables => true;

        protected override bool SupportsItemLabels => true;

        protected override float TotalDepthLink => 0f;

        protected override float TotalHeightLink => 0f;

        protected override float TotalWidthLink => 0f;

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

        protected override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        ///     used internally , do not call this method
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying) HookEvents();
            Invalidate();
        }


        protected void AddBillboardText(string cat, BillboardText text)
        {
            List<BillboardText> addTo;
            if (mTexts.TryGetValue(cat, out addTo) == false)
            {
                addTo = new List<BillboardText>();
                mTexts.Add(cat, addTo);
            }

            addTo.Add(text);
        }

        private void HookEvents()
        {
            Data.ProperyUpdated -= DataUpdated;
            Data.ProperyUpdated += DataUpdated;
            ((IInternalRadarData)Data).InternalDataSource.DataStructureChanged -= StructureUpdated;
            ((IInternalRadarData)Data).InternalDataSource.DataStructureChanged += StructureUpdated;
            ((IInternalRadarData)Data).InternalDataSource.DataValueChanged -= ValueChanged;
            ;
            ((IInternalRadarData)Data).InternalDataSource.DataValueChanged += ValueChanged;
        }

        private void ValueChanged(object sender, ChartDataSourceBase.DataValueChangedEventArgs e)
        {
            Invalidate();
        }

        private void StructureUpdated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void DataUpdated()
        {
            Invalidate();
        }

        protected override void ClearChart()
        {
            base.ClearChart();
            mActiveTexts.Clear();
            mTexts.Clear();
        }

        public bool ItemToWorldPosition(string group, double amount, out Vector3 worldposition)
        {
            worldposition = Vector3.zero;
            if (mDirections == null || mDirections.Length == 0)
                return false;
            var index = Data.GetGroupIndex(group);
            var dir = mDirections[index];
            var max = Data.GetMaxValue();
            worldposition = (float)(amount / max) * Radius * dir;
            worldposition = transform.TransformPoint(worldposition);
            return true;
        }

        public bool SnapWorldPointToPosition(Vector3 worldSpace, out string group, out double amount)
        {
            group = null;
            amount = 0f;
            if (mDirections == null || mDirections.Length == 0)
                return false;
            var pos = transform.InverseTransformPoint(worldSpace);
            pos.z = 0;

            //   Vector3 dir = mDirections[0];
            group = Data.GetGroupName(0);
            if (Math.Abs(pos.x) < 0.001f && Math.Abs(pos.y) < 0.001f)
            {
                //zero vector do nothing we are taking the first direction in the array
            }
            else
            {
                var dot = float.MinValue;
                for (var i = 0; i < mDirections.Length; i++)
                {
                    var newDot = Vector3.Dot(mDirections[i], pos);
                    if (newDot > dot)
                    {
                        dot = newDot;
                        //        dir = mDirections[i];
                        group = Data.GetGroupName(i);
                    }
                }
            }

            var mag = pos.magnitude;
            var max = Data.GetMaxValue();
            amount = mag / Radius * max;
            amount = Math.Max(0, Math.Min(max, amount));
            return true;
        }

        public override void InternalGenerateChart()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            ClearChart();

            base.InternalGenerateChart();

            if (((IInternalRadarData)Data).InternalDataSource == null)
                return;

            var data = ((IInternalRadarData)Data).InternalDataSource.getRawData();
            var rowCount = data.GetLength(0);
            var columnCount = data.GetLength(1);

            //restrict to 3 groups
            if (rowCount < 3)
                return;

            mDirections = new Vector3[rowCount];
            var angles = new float[rowCount];

            for (var i = 0; i < rowCount; i++)
            {
                var angle = (float)(i / (float)rowCount * Math.PI * 2f) + Angle * Mathf.Deg2Rad;
                angles[i] = angle;
                mDirections[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            }

            var path = new Vector3[rowCount];
            var zAdd = Vector3.zero;

            for (var i = 0; i < TotalAxisDevisions; i++)
            {
                var rad = Radius * ((i + 1) / (float)TotalAxisDevisions);
                for (var j = 0; j < rowCount; j++)
                    path[j] = mDirections[j] * rad + zAdd;
                //  path[rowCount] = path[0];
                zAdd.z += AxisAdd;
                var axisObject = CreateAxisObject(AxisThickness, path);
                mAxisObjects.Add(axisObject);
                axisObject.transform.SetParent(transform, false);
            }

            if (mGroupLabels != null && mGroupLabels.isActiveAndEnabled)
                for (var i = 0; i < rowCount; i++)
                {
                    var group = Data.GetGroupName(i);
                    var basePosition = mDirections[i] * Radius;
                    var breadthAxis = Vector3.Cross(mDirections[i], Vector3.forward);
                    var position = basePosition + mDirections[i] * mGroupLabels.Seperation;
                    position += breadthAxis * mGroupLabels.Location.Breadth;
                    position += new Vector3(0f, 0f, mGroupLabels.Location.Depth);
                    var toSet = mGroupLabels.TextFormat.Format(group, "", "");
                    var billboard = ChartCommon.CreateBillboardText(null, mGroupLabels.TextPrefab, transform, toSet,
                        position.x, position.y, position.z, angles[i], transform, hideHierarchy, mGroupLabels.FontSize,
                        mGroupLabels.FontSharpness);
                    billboard.UserData = group;
                    TextController.AddText(billboard);
                }

            var maxValue = Data.GetMaxValue();
            var minValue = Data.GetMinValue();

            if (maxValue > 0.000001f)
            {
                for (var i = 0; i < columnCount; i++)
                {
                    var finalMaxValue = DataSource.GetCategoryMaxValue(i);
                    if (finalMaxValue < 0f)
                        finalMaxValue = maxValue;
                    for (var j = 0; j < rowCount; j++)
                    {
                        var rad = (float)((data[j, i] - minValue) / (finalMaxValue - minValue)) * Radius;
                        path[j] = mDirections[j] * rad;
                    }

                    //  path[rowCount] = path[0];
                    var category = CreateCategoryObject(path, i);
                    category.transform.SetParent(transform, false);
                }

                if (mItemLabels != null && mItemLabels.isActiveAndEnabled)
                {
                    var angle = mItemLabels.Location.Breadth;
                    var blend = angle / 360f;
                    blend -= Mathf.Floor(blend);
                    blend *= rowCount;
                    var index = (int)blend;
                    var nextIndex = (index + 1) % rowCount;
                    blend = blend - Mathf.Floor(blend);
                    for (var i = 0; i < TotalAxisDevisions; i++)
                    {
                        var factor = (i + 1) / (float)TotalAxisDevisions;
                        var rad = Radius * factor + mItemLabels.Seperation;
                        var value = ChartAdancedSettings.Instance.FormatFractionDigits(mItemLabels.FractionDigits,
                            Mathf.Lerp((float)minValue, (float)maxValue, factor), CustomNumberFormat);
                        var position = Vector3.Lerp(mDirections[index], mDirections[nextIndex], blend) * rad;
                        position.z = mItemLabels.Location.Depth;
                        var toSet = mItemLabels.TextFormat.Format(value, "", "");
                        var billboard = ChartCommon.CreateBillboardText(null, mItemLabels.TextPrefab, transform, toSet,
                            position.x, position.y, position.z, 0f, transform, hideHierarchy, mItemLabels.FontSize,
                            mItemLabels.FontSharpness);
                        billboard.UserData = (float)(maxValue * factor);
                        TextController.AddText(billboard);
                    }
                }
            }
        }

        protected override void OnItemSelected(object userData)
        {
            base.OnItemSelected(userData);
            var args = userData as RadarEventArgs;
            if (args == null)
                return;
            mOccupiedCateogies.Add(args.Category);
            if (PointClicked != null)
                PointClicked.Invoke(args);
        }

        protected override void OnItemLeave(object userData, string type)
        {
            base.OnItemLeave(userData, type);
            var args = userData as RadarEventArgs;
            foreach (var t in mActiveTexts)
            foreach (var effect in t.UIText.GetComponents<ChartItemEffect>())
            {
                if (t.UIText == null)
                    continue;
                effect.TriggerOut(false);
            }

            mActiveTexts.Clear();

            var category = args.Category;
            mOccupiedCateogies.Remove(category);
            mOccupiedCateogies.RemoveWhere(x => !Data.HasCategory(x));
            if (mOccupiedCateogies.Count == 0)
                if (NonHovered != null)
                    NonHovered.Invoke();
        }

        protected override void OnItemHoverted(object userData)
        {
            base.OnItemHoverted(userData);
            List<BillboardText> catgoryTexts;
            var args = userData as RadarEventArgs;
            if (args == null)
                return;
            foreach (var t in mActiveTexts)
            {
                if (t.UIText == null)
                    continue;
                foreach (var effect in t.UIText.GetComponents<ChartItemEffect>())
                    effect.TriggerOut(false);
            }

            mActiveTexts.Clear();


            if (mTexts.TryGetValue(args.Category, out catgoryTexts))
                if (args.Index < catgoryTexts.Count)
                {
                    var b = catgoryTexts[args.Index];
                    mActiveTexts.Add(b);
                    var t = b.UIText;
                    if (t != null)
                        foreach (var effect in t.GetComponents<ChartItemEffect>())
                            effect.TriggerIn(false);
                }

            mOccupiedCateogies.Add(args.Category);
            if (PointHovered != null)
                PointHovered.Invoke(args);
        }

        protected abstract GameObject CreateCategoryObject(Vector3[] path, int category);
        protected abstract GameObject CreateAxisObject(float thickness, Vector3[] path);

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

        public class RadarEventArgs
        {
            public RadarEventArgs(string category, string group, double value, Vector3 position, int index)
            {
                Position = position;
                Category = category;
                Group = group;
                Value = value;
                Index = index;
            }

            public int Index { get; }
            public string Category { get; }
            public string Group { get; }
            public double Value { get; }
            public Vector3 Position { get; }
        }

        [Serializable]
        public class RadarEvent : UnityEvent<RadarEventArgs>
        {
        }
    }
}