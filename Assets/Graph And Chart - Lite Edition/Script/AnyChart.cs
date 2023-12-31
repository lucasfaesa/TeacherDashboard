#define Graph_And_Chart_PRO
using System;
using System.Collections.Generic;
using ChartAndGraph.Axis;
using UnityEngine;
using UnityEngine.Events;

namespace ChartAndGraph
{
    /// <summary>
    ///     this is a base class for all chart types
    /// </summary>
    [Serializable]
    public abstract class AnyChart : MonoBehaviour, IInternalUse, ISerializationCallbackReceiver
    {
        public enum FitAlign
        {
            StartXCenterY,
            CenterXStartY,
            CenterXCenterY,
            StartXStartY
        }

        public enum FitOrientation
        {
            Normal,
            Vertical,
            VerticalOpopsite
        }

        public enum FitType
        {
            None,
            Aspect,
            Height,
            Width
        }

        private static List<GameObject> toMove = new();

        [SerializeField] [HideInInspector] protected GameObject mPreviewObject;

        public UnityEvent OnRedraw = new();

        [SerializeField] private bool keepOrthoSize;

        [SerializeField] private bool paperEffectText;

        [SerializeField] private bool vRSpaceText;

        [SerializeField] private float vRSpaceScale = 0.1f;

        [SerializeField] private bool maintainLabelSize;

        private Func<DateTime, string> customDateTimeFormat;

        private Func<double, int, string> customNumberFormat;

        protected bool hideHierarchy = true;
        protected GameObject HorizontalCustomDivisions;
        protected GameObject HorizontalMainDevisions;
        protected GameObject HorizontalSubDevisions;

        // the axis generated for the chart
        private List<IAxisGenerator> mAxis = new();
        protected CategoryLabels mCategoryLabels;
        protected GameObject mFixPosition;

        private bool mGenerating;
        protected GroupLabels mGroupLabels;
        protected HorizontalAxis mHorizontalAxis;
        private HashSet<double> mHorizontalCustomAxis = new(), mVerticalCustomAxis = new();

        private HashSet<object> mHovered = new();
        protected ItemLabels mItemLabels;
        private Vector2 mLastSetSize = Vector2.zero;
        private bool mRealtimeOnNextUpdate;


        private TextController mTextController;
        protected VerticalAxis mVerticalAxis;
        protected GameObject VerticalCustomDevisions;
        protected GameObject VerticalMainDevisions;
        protected GameObject VerticalSubDevisions;

        public Func<double, int, string> CustomNumberFormat
        {
            get => customNumberFormat;
            set
            {
                customNumberFormat = value;
                Invalidate();
            }
        }

        public Func<DateTime, string> CustomDateTimeFormat
        {
            get => customDateTimeFormat;
            set
            {
                customDateTimeFormat = value;
                Invalidate();
            }
        }

        public Dictionary<DoubleVector3, KeyValuePair<string, string>> VectorValueToStringMap { get; } =
            new(ChartCommon.DefaultDoubleVector3Comparer);

        public Dictionary<double, string> VerticalValueToStringMap { get; } = new(ChartCommon.DefaultDoubleComparer);

        public Dictionary<double, string> HorizontalValueToStringMap { get; } = new(ChartCommon.DefaultDoubleComparer);

        protected virtual Camera TextCameraLink => null;

        protected virtual float TextIdleDistanceLink => 20f;

        public bool KeepOrthoSize
        {
            get => keepOrthoSize;
            set
            {
                KeepOrthoSize = value;
                GenerateChart();
            }
        }

        public bool PaperEffectText
        {
            get => paperEffectText;
            set
            {
                paperEffectText = value;
                GenerateChart();
            }
        }

        public bool VRSpaceText
        {
            get => vRSpaceText;
            set
            {
                vRSpaceText = value;
                GenerateChart();
            }
        }

        public float VRSpaceScale
        {
            get => vRSpaceScale;
            set
            {
                vRSpaceScale = value;
                GenerateChart();
            }
        }

        public bool MaintainLabelSize
        {
            get => maintainLabelSize;
            set
            {
                maintainLabelSize = value;
                GenerateChart();
            }
        }

        protected bool IsUnderCanvas { get; private set; }
        protected bool CanvasChanged { get; private set; }

        /// <summary>
        ///     override this in a dervied class to set the total depth of the chart, this is used by 3d axis components to
        ///     determine automatic size
        /// </summary>
        protected abstract float TotalDepthLink { get; }

        /// <summary>
        ///     override this in a dervied class to set the total height of the chart, this is used by axis components and by size
        ///     fitting for canvas
        /// </summary>
        protected abstract float TotalHeightLink { get; }

        /// <summary>
        ///     override this in a dervied class to set the total width of the chart, this is used by axis components and by size
        ///     fitting for canvas
        /// </summary>
        protected abstract float TotalWidthLink { get; }

        protected abstract IChartData DataLink { get; }

        /// <summary>
        ///     true if the class type is meant for use with canvas
        /// </summary>
        public abstract bool IsCanvas { get; }

        public float TotalWidth => TotalWidthLink;

        public float TotalHeight => TotalHeightLink;

        public float TotalDepth => TotalDepthLink;

        protected bool Invalidating { get; private set; }

        protected TextController TextController
        {
            get
            {
                EnsureTextController();
                return mTextController;
            }
            private set => mTextController = value;
        }

        /// <summary>
        ///     This method returns information used for legened creation
        /// </summary>
        protected abstract LegenedData LegendInfo { get; }

        public abstract bool SupportRealtimeGeneration { get; }

        protected GameObject FixPosition => mFixPosition;

        protected virtual ChartMagin MarginLink => new(0f, 0f, 0f, 0f);
        protected virtual Vector3 CanvasFitOffset => new(0.5f, 0.5f, 0f);
        protected virtual bool ShouldFitCanvas => false;
        protected virtual float FitZRotationCanvas => 0;
        protected virtual FitType FitAspectCanvas => FitType.None;
        protected virtual FitAlign FitAlignCanvas => FitAlign.CenterXCenterY;

        public void Awake()
        {
            ClearChart();
        }

        protected virtual void Start()
        {
            if (gameObject.activeInHierarchy == false)
                return;
            DoCanvas(true);
            EnsureTextController();
        }

        protected virtual void Update()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            DoCanvas(false);

            if (Invalidating)
            {
                // complete redraw preceeds realtime
                GenerateChart();
            }
            else if (mRealtimeOnNextUpdate)
            {
                GenerateRealtime();
                InvokeOnRedraw();
            }

            Invalidating = false; // graph is invalidated set this back to false
            mRealtimeOnNextUpdate = false;
            DataLink.Update();

            if (IsCanvas)
            {
                var trans = GetComponent<RectTransform>();
                if (trans != null && trans.hasChanged)
                    if (mLastSetSize != trans.rect.size)
                        Invalidate();
            }
        }

        protected virtual void LateUpdate()
        {
        }


        /// <summary>
        ///     when enabling a chart , all chartItem components must be activated. Every unity gameobject create by graph and
        ///     chart is marked as a ChartItem
        /// </summary>
        protected virtual void OnEnable()
        {
            var children = GetComponentsInChildren<ChartItem>(true);
            for (var i = 0; i < children.Length; ++i)
                if (children[i] != null)
                    if (children[i].gameObject != gameObject)
                        children[i].gameObject.SetActive(true);
        }

        /// <summary>
        ///     when disbaling a chart , all chartItem components must be disabled. Every unity gameobject create by graph and
        ///     chart is marked as a ChartItem
        /// </summary>
        protected virtual void OnDisable()
        {
            var children = GetComponentsInChildren<ChartItem>();
            for (var i = 0; i < children.Length; ++i)
                if (children[i] != null)
                    if (children[i].gameObject != gameObject)
                        children[i].gameObject.SetActive(false);
        }

        protected virtual void OnTransformParentChanged()
        {
            Invalidate();
        }

        /// <summary>
        ///     used internally, do not call this method
        /// </summary>
        protected virtual void OnValidate()
        {
            if (gameObject.activeInHierarchy == false)
                return;
            ValidateProperties();
            DoCanvas(true);

            //  EnsureTextController();
        }

        HashSet<double> IInternalUse.VerticalCustomAxis => mVerticalCustomAxis;
        HashSet<double> IInternalUse.HorizontalCustomAxis => mHorizontalCustomAxis;

        /// <summary>
        ///     Keeps all the chart elements hidden from the editor and the inspector
        /// </summary>
        bool IInternalUse.HideHierarchy => hideHierarchy;

        public void RemoveHorizontalAxisDivision(double pos)
        {
            mHorizontalCustomAxis.Remove(pos);
            Invalidate();
        }

        public void AddHorizontalAxisDivision(double pos)
        {
            mHorizontalCustomAxis.Add(pos);
            Invalidate();
        }

        public void ClearHorizontalCustomDivisions()
        {
            mHorizontalCustomAxis.Clear();
            Invalidate();
        }

        public void RemoveVerticalAxisDivision(double pos)
        {
            mVerticalCustomAxis.Remove(pos);
            Invalidate();
        }

        public void AddVerticalAxisDivision(double pos)
        {
            mVerticalCustomAxis.Add(pos);
            Invalidate();
        }

        public void ClearVerticalCustomDivisions()
        {
            mVerticalCustomAxis.Clear();
            Invalidate();
        }

        private void AxisChanged(object sender, EventArgs e)
        {
            GenerateChart();
        }

        /// <summary>
        ///     This method is called every time a property of the chart has changed. This would ussually trigger data validation
        ///     and chart rebuilding
        /// </summary>
        protected virtual void OnPropertyUpdated()
        {
            ValidateProperties();
        }

        private void Labels_OnDataChanged(object sender, EventArgs e)
        {
            OnLabelSettingsSet();
        }

        private void Labels_OnDataUpdate(object sender, EventArgs e)
        {
            OnLabelSettingChanged();
        }

        protected virtual void OnLabelSettingChanged()
        {
            Invalidate();
        }

        protected virtual double GetScrollOffset(int axis)
        {
            return 0.0;
        }

        protected void FixAxisLabels()
        {
            for (var i = 0; i < mAxis.Count; ++i)
                mAxis[i].FixLabels(this);
        }

        protected virtual void OnAxisValuesChanged()
        {
        }

        protected virtual void OnLabelSettingsSet()
        {
        }

        /// <summary>
        ///     this method will detect changes in the parent canvas , if the canvas changed the chart will be regenerated
        /// </summary>
        /// <param name="start"></param>
        private void DoCanvas(bool start)
        {
            var parentCanvas = GetComponentInParent<Canvas>();
            var prev = IsUnderCanvas;
            IsUnderCanvas = parentCanvas != null;
            if (IsUnderCanvas == false)
                return;
            if (start == false)
                if (IsUnderCanvas != prev)
                {
                    CanvasChanged = true;
                    GenerateChart();
                    CanvasChanged = false;
                }
        }

        /// <summary>
        ///     generates a text controller object for this chart
        /// </summary>
        private void CreateTextController()
        {
            var obj = new GameObject("textController", typeof(RectTransform));
            ;
            ChartCommon.HideObject(obj, hideHierarchy);
            obj.transform.SetParent(transform);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localPosition = Vector3.zero;
            mTextController = obj.AddComponent<TextController>();
            mTextController.mParent = this;
        }

        /// <summary>
        ///     This method ensures that the chart has a TextController object under it's hirarchy , The TextController object is
        ///     responsible for text placing and billboarding
        /// </summary>
        protected void EnsureTextController()
        {
            if (mTextController != null)
            {
                mTextController.GlobalRotation = -FitZRotationCanvas;
                return;
            }

            CreateTextController();
            mTextController.GlobalRotation = -FitZRotationCanvas;
        }

        /// <summary>
        ///     call this to invalidate a chart in reatime , this will update only some parts of the chart depending on
        ///     implementation.
        ///     This method will only work if SupportRealtimeGeneration returns true
        /// </summary>
        protected virtual void InvalidateRealtime()
        {
            if (mGenerating)
                return;
            mRealtimeOnNextUpdate = true;
        }

        /// <summary>
        ///     call this to invalidate the chart and make the chart rebuild itself completly in the next update call
        /// </summary>
        public virtual void Invalidate()
        {
            if (mGenerating)
                return;
            Invalidating = true;
        }

        /// <summary>
        ///     Given an axis component , this method would return true if that axis is supported for this chart type.
        ///     for example: if the chart supports only horizontal axis , this method should return true for horizontal axis and
        ///     false otherwise
        ///     This method is used by the axis drawing method to determine if this chart type provides data for an axis. (if not
        ///     then the axis is drawn without values
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        protected abstract bool HasValues(AxisBase axis);

        /// <summary>
        ///     returns the maximum value for the specified axis , or 0 if HasValues(axis) returns false.
        ///     the maximum value is usually determined by the chart data source , and can be mannualy overriden by the user
        /// </summary>
        protected abstract double MaxValue(AxisBase axis);

        /// <summary>
        ///     returns the minimum value for the specified axis , or 0 if HasValues(axis) returns false.
        ///     the minimum value is usually determined by the chart data source , and can be mannualy overriden by the user
        /// </summary>
        protected abstract double MinValue(AxisBase axis);


        /// <summary>
        ///     call this in a dervied class in order to invoke the OnRedraw event. The OnRedraw event should be called any time a
        ///     marker position is about to change.
        /// </summary>
        protected void InvokeOnRedraw()
        {
            if (OnRedraw != null)
                OnRedraw.Invoke();
        }

        public virtual void GenerateRealtime()
        {
        }

        /// <summary>
        ///     This method generates a chart by calling InternalGenerateChart
        /// </summary>
        public void GenerateChart()
        {
            if (mGenerating) // avoid nested calls to generate chart
                return;
            mGenerating = true;
            InternalGenerateChart(); // call derivative class implementation
            var children = gameObject.GetComponentsInChildren<Transform>();
            // setup the layer for all child object so it matches the parent
            for (var i = 0; i < children.Length; i++)
            {
                var t = children[i];
                if (t == null || t.gameObject == null)
                    continue;
                t.gameObject.layer = gameObject.layer;
            }

            TextController.transform.SetAsLastSibling();
            if (IsCanvas)
                FitCanvas();
            InvokeOnRedraw();
            mGenerating = false;
        }

        /// <summary>
        ///     override this method in a derived class to generate a chart type.
        /// </summary>
        public virtual void InternalGenerateChart()
        {
            if (gameObject.activeInHierarchy == false)
                return;
            Invalidating = false;
            if (ChartGenerated != null)
                ChartGenerated();
        }

        /// <summary>
        ///     override this method in a dervied class to add custom clearing for any chart type. default implementation deletes
        ///     all chartItem components from this transform
        /// </summary>
        protected virtual void ClearChart()
        {
//#if UNITY_2018_3_OR_NEWER
//#if UNITY_EDITOR

//            //  if (Application.isEditor == true && Application.isPlaying == false)
//            //  {
//            var path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
//            if (!(path == null || path.Trim().Length == 0))
//            {
//                // Load the contents of the Prefab Asset.
//                GameObject contentsRoot = UnityEditor.PrefabUtility.LoadPrefabContents(path);

//                bool save = false;
//                // Modify Prefab contents.
//                foreach (var item in contentsRoot.GetComponentsInChildren<ChartItem>())
//                {
//                    if (item == null)
//                        continue;
//                    if (item.gameObject != null)
//                    {
//                        save = true;
//                        DestroyImmediate(item.gameObject);
//                    }
//                }
//                if (save)
//                {
//                    //  try
//                    //  {
//                    UnityEditor.PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
//                    //  }
//                    //  catch (Exception e)
//                    //  {
//                    //  }

//                }

//                UnityEditor.PrefabUtility.UnloadPrefabContents(contentsRoot);
//            }
//            // }
//#endif
//#endif
            mHovered.Clear();

            if (TextController != null) // destroy all child text object
            {
                TextController.DestroyAll();
                TextController.transform.SetParent(transform,
                    false); // the text controller my not be a direct child of this gameobject , make it so that it is.
            }

            // destroy all child ChartItem objects
            var children = GetComponentsInChildren<ChartItem>();
            for (var i = 0; i < children.Length; ++i)
                if (children[i] != null)
                {
                    if (TextController != null && children[i].gameObject == TextController.gameObject)
                        continue;
                    if (children[i].gameObject.GetComponent<ChartItemNoDelete>() != null)
                        continue;
                    if (children[i].gameObject.GetComponentInParent<AnyChart>() != this)
                        continue;
                    if (children[i].gameObject != gameObject)
                        ChartCommon.SafeDestroy(children[i].gameObject);
                }

            // ensure the text controller has been created ( it is generated only when the graph chart is first created)
            EnsureTextController();


            //destroy all axis components in this chart
            for (var i = 0; i < mAxis.Count; i++)
                if (mAxis[i] != null && mAxis[i].This() != null)
                    ChartCommon.SafeDestroy(mAxis[i].GetGameObject());
            mAxis.Clear();
        }

        private void FitCanvas()
        {
            var trans = GetComponent<RectTransform>();
            mLastSetSize = trans.rect.size;
            if (ShouldFitCanvas == false)
                return;
            //   if (FitAspectCanvas ==  FitType.None)
            //       return;
            var margin = MarginLink;
            if (mFixPosition != null)
                ChartCommon.SafeDestroy(mFixPosition);
            var fixPosition = new GameObject();
            var fixRect = fixPosition.AddComponent<RectTransform>();
            mFixPosition = fixPosition;
            ChartCommon.HideObject(fixPosition, hideHierarchy);
            fixPosition.AddComponent<ChartItem>();


            var totalWidth = TotalWidthLink; // + margin.Left + margin.Right;
            var totalHeight = TotalHeightLink; // + margin.Top + margin.Bottom;


            fixRect.SetParent(transform, false);
            fixRect.localPosition = Vector3.zero;

            fixRect.anchorMin = new Vector2(0f, 0f);
            fixRect.anchorMax = new Vector2(0f, 0f);
            fixRect.pivot = new Vector2(0f, 0f);
            fixRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
            fixRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            if (FitZRotationCanvas == 90 || FitZRotationCanvas == -90)
            {
                totalHeight = TotalWidthLink;
                totalWidth = TotalHeightLink;
            }

            var xAnchor = 0.5f;
            var yAnchor = 0.5f;


            if (FitAlignCanvas == FitAlign.StartXCenterY || FitAlignCanvas == FitAlign.StartXStartY) xAnchor = 0f;

            if (FitAlignCanvas == FitAlign.CenterXStartY || FitAlignCanvas == FitAlign.StartXStartY)
                yAnchor = 0f;


            fixPosition.transform.localScale = new Vector3(1f, 1f, 1f);
            fixPosition.transform.SetSiblingIndex(0);
            toMove.Clear();

            for (var i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = trans.GetChild(i).gameObject;
                if (child == null)
                    continue;
                if (child == fixPosition)
                    continue;
                if (child.GetComponent<AnyChart>() != null)
                    continue;
                if (child.GetComponent<ChartItem>() == null)
                    continue;
                toMove.Add(child);
            }

            foreach (var obj in toMove) obj.transform.SetParent(fixPosition.transform, false);

            toMove.Clear();


            fixRect.anchorMin = new Vector2(xAnchor, yAnchor);
            fixRect.anchorMax = new Vector2(xAnchor, yAnchor);
            fixRect.pivot = new Vector2(xAnchor, yAnchor);

            if (totalWidth <= 0 || TotalHeight <= 0)
                return;

            var widthScale = (trans.rect.size.x - margin.Left - margin.Right) / totalWidth;
            var heightScale = (trans.rect.size.y - margin.Top - margin.Bottom) / totalHeight;
            var uniformScale = Math.Min(widthScale, heightScale);
            if (FitAspectCanvas == FitType.Height)
                uniformScale = heightScale;
            else if (FitAspectCanvas == FitType.Width)
                uniformScale = widthScale;
            else if (FitAspectCanvas == FitType.None)
                uniformScale = 1f;

            fixRect.localScale = new Vector3(uniformScale, uniformScale, 1f);
            if (MaintainLabelSize)
                TextController.SetInnerScale(1f / uniformScale);
            else
                TextController.SetInnerScale(1f);


            fixRect.anchoredPosition = new Vector3(Mathf.Lerp(margin.Left, -margin.Right, xAnchor),
                Mathf.Lerp(margin.Bottom, -margin.Top, yAnchor), 0f);
            fixRect.localRotation = Quaternion.Euler(0, 0, FitZRotationCanvas);
        }

        /// <summary>
        ///     called by a derived class to indicate that the mouse has left any selectable object in the chart.
        /// </summary>
        protected virtual void OnNonHoverted()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="userData"></param>
        protected virtual void OnItemLeave(object userData, string type)
        {
            if (mHovered.Count == 0)
                return;
            mHovered.Remove(userData);
            if (mHovered.Count == 0)
                OnNonHoverted();
        }

        protected virtual void OnItemSelected(object userData)
        {
        }

        protected virtual void OnItemHoverted(object userData)
        {
            mHovered.Add(userData);
        }

        protected internal virtual IAxisGenerator InternalUpdateAxis(ref GameObject axisObject, AxisBase axisBase,
            ChartOrientation axisOrientation, int divType, bool forceRecreate, double scrollOffset)
        {
            IAxisGenerator res = null;
            if (axisObject == null || forceRecreate || CanvasChanged)
            {
                ChartCommon.SafeDestroy(axisObject);
                GameObject axis = null;
                if (IsUnderCanvas)
                {
                    axis = ChartCommon.CreateCanvasChartItem();
                    axis.transform.SetParent(transform, false);
                    var rect = axis.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(0f, 0f);
                    rect.localScale = new Vector3(1f, 1f, 1f);
                    rect.localRotation = Quaternion.identity;
                    rect.anchoredPosition = new Vector3();
                }
                else
                {
                    axis = ChartCommon.CreateChartItem();
                    axis.transform.SetParent(transform, false);
                    axis.transform.localScale = new Vector3(1f, 1f, 1f);
                    axis.transform.localRotation = Quaternion.identity;
                    axis.transform.localPosition = new Vector3();
                }

                axisBase.ClearFormats();

                axis.layer = gameObject.layer; // put the axis on the same layer as the chart
                ChartCommon.HideObject(axis, hideHierarchy);
                axisObject = axis;
                if (IsUnderCanvas)
                    res = axis.AddComponent<CanvasAxisGenerator>();
                else
                    res = axis.AddComponent<AxisGenerator>();
            }
            else
            {
                if (IsUnderCanvas)
                    res = axisObject.GetComponent<CanvasAxisGenerator>();
                else
                    res = axisObject.GetComponent<AxisGenerator>();
            }

            res.SetAxis(scrollOffset, this, axisBase, axisOrientation, divType);

            //      axisObject.transform.localScale = new Vector3(1f, 1f, 1f);
            //       axisObject.transform.localRotation = Quaternion.identity;
            //       axisObject.transform.localPosition = new Vector3();
            return res;
        }

        protected virtual void ValidateProperties()
        {
            if (mItemLabels != null)
                mItemLabels.ValidateProperties();
            if (mCategoryLabels != null)
                mCategoryLabels.ValidateProperties();
            if (mGroupLabels != null)
                mGroupLabels.ValidateProperties();
            if (mHorizontalAxis != null)
                mHorizontalAxis.ValidateProperties();
            if (mVerticalAxis != null)
                mVerticalAxis.ValidateProperties();
        }

        protected void GenerateAxis(bool force)
        {
            mAxis.Clear();
            if (mVerticalAxis)
            {
                var scroll = GetScrollOffset(1);
                var main = InternalUpdateAxis(ref VerticalMainDevisions, mVerticalAxis, ChartOrientation.Vertical, 0,
                    force, scroll);
                var sub = InternalUpdateAxis(ref VerticalSubDevisions, mVerticalAxis, ChartOrientation.Vertical, 1,
                    force, scroll);
                var custom = InternalUpdateAxis(ref VerticalCustomDevisions, mVerticalAxis, ChartOrientation.Vertical,
                    2, force, scroll);
                if (main != null)
                    mAxis.Add(main);
                if (sub != null)
                    mAxis.Add(sub);
                if (custom != null)
                    mAxis.Add(custom);
            }

            if (mHorizontalAxis)
            {
                var scroll = GetScrollOffset(0);
                var main = InternalUpdateAxis(ref HorizontalMainDevisions, mHorizontalAxis, ChartOrientation.Horizontal,
                    0, force, scroll);
                var sub = InternalUpdateAxis(ref HorizontalSubDevisions, mHorizontalAxis, ChartOrientation.Horizontal,
                    1, force, scroll);
                var custom = InternalUpdateAxis(ref HorizontalCustomDivisions, mHorizontalAxis,
                    ChartOrientation.Horizontal, 2, force, scroll);
                if (main != null)
                    mAxis.Add(main);
                if (sub != null)
                    mAxis.Add(sub);
                if (custom != null)
                    mAxis.Add(custom);
            }
        }

        private event Action ChartGenerated;

        protected void RaiseChartGenerated()
        {
            if (ChartGenerated != null)
                ChartGenerated();
        }

        #region Internal Use

        event Action IInternalUse.Generated
        {
            add => ChartGenerated += value;
            remove => ChartGenerated -= value;
        }

        void IInternalUse.InternalItemSelected(object userData)
        {
            OnItemSelected(userData);
        }

        void IInternalUse.InternalItemLeave(object userData)
        {
            OnItemLeave(userData, "unknown");
        }

        void IInternalUse.InternalItemHovered(object userData)
        {
            OnItemHoverted(userData);
        }

        void IInternalUse.CallOnValidate()
        {
            OnValidate();
        }

        /// <summary>
        ///     Label settings for the chart items
        /// </summary>
        ItemLabels IInternalUse.ItemLabels
        {
            get => mItemLabels;
            set
            {
                if (mItemLabels != null)
                {
                    ((IInternalSettings)mItemLabels).InternalOnDataUpdate -= Labels_OnDataUpdate;
                    ((IInternalSettings)mItemLabels).InternalOnDataChanged -= Labels_OnDataChanged;
                }

                mItemLabels = value;
                if (mItemLabels != null)
                {
                    ((IInternalSettings)mItemLabels).InternalOnDataUpdate += Labels_OnDataUpdate;
                    ((IInternalSettings)mItemLabels).InternalOnDataChanged += Labels_OnDataChanged;
                }

                OnLabelSettingsSet();
            }
        }

        VerticalAxis IInternalUse.VerticalAxis
        {
            get => mVerticalAxis;
            set
            {
                if (mVerticalAxis != null)
                {
                    ((IInternalSettings)mVerticalAxis).InternalOnDataChanged -= AxisChanged;
                    ((IInternalSettings)mVerticalAxis).InternalOnDataUpdate -= AxisChanged;
                }

                mVerticalAxis = value;
                if (mVerticalAxis != null)
                {
                    ((IInternalSettings)mVerticalAxis).InternalOnDataChanged += AxisChanged;
                    ((IInternalSettings)mVerticalAxis).InternalOnDataUpdate += AxisChanged;
                }

                OnAxisValuesChanged();
            }
        }


        HorizontalAxis IInternalUse.HorizontalAxis
        {
            get => mHorizontalAxis;
            set
            {
                if (mHorizontalAxis != null)
                {
                    ((IInternalSettings)mHorizontalAxis).InternalOnDataChanged -= AxisChanged;
                    ((IInternalSettings)mHorizontalAxis).InternalOnDataUpdate -= AxisChanged;
                }

                mHorizontalAxis = value;
                if (mHorizontalAxis != null)
                {
                    ((IInternalSettings)mHorizontalAxis).InternalOnDataChanged += AxisChanged;
                    ((IInternalSettings)mHorizontalAxis).InternalOnDataUpdate += AxisChanged;
                }

                OnAxisValuesChanged();
            }
        }

        /// <summary>
        ///     Label settings for the chart categories
        /// </summary>
        CategoryLabels IInternalUse.CategoryLabels
        {
            get => mCategoryLabels;
            set
            {
                if (mCategoryLabels != null)
                {
                    ((IInternalSettings)mCategoryLabels).InternalOnDataUpdate -= Labels_OnDataUpdate;
                    ((IInternalSettings)mCategoryLabels).InternalOnDataChanged -= Labels_OnDataChanged;
                }

                mCategoryLabels = value;
                if (mCategoryLabels != null)
                {
                    ((IInternalSettings)mCategoryLabels).InternalOnDataUpdate += Labels_OnDataUpdate;
                    ((IInternalSettings)mCategoryLabels).InternalOnDataChanged += Labels_OnDataChanged;
                }

                OnLabelSettingsSet();
            }
        }


        /// <summary>
        ///     Label settings for group labels
        /// </summary>
        GroupLabels IInternalUse.GroupLabels
        {
            get => mGroupLabels;
            set
            {
                if (mGroupLabels != null)
                {
                    ((IInternalSettings)mGroupLabels).InternalOnDataUpdate -= Labels_OnDataUpdate;
                    ((IInternalSettings)mGroupLabels).InternalOnDataChanged -= Labels_OnDataChanged;
                }

                mGroupLabels = value;
                if (mGroupLabels != null)
                {
                    ((IInternalSettings)mGroupLabels).InternalOnDataUpdate += Labels_OnDataUpdate;
                    ((IInternalSettings)mGroupLabels).InternalOnDataChanged += Labels_OnDataChanged;
                }

                OnLabelSettingsSet();
            }
        }

        TextController IInternalUse.InternalTextController => TextController;
        LegenedData IInternalUse.InternalLegendInfo => LegendInfo;

        Camera IInternalUse.InternalTextCamera => TextCameraLink;

        float IInternalUse.InternalTextIdleDistance => TextIdleDistanceLink;

        bool IInternalUse.InternalHasValues(AxisBase axis)
        {
            return HasValues(axis);
        }

        double IInternalUse.InternalMaxValue(AxisBase axis)
        {
            return MaxValue(axis);
        }

        double IInternalUse.InternalMinValue(AxisBase axis)
        {
            return MinValue(axis);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DataLink.OnBeforeSerialize();
            OnBeforeSerializeEvent();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            DataLink.OnAfterDeserialize();
        }

        protected virtual void OnBeforeSerializeEvent()
        {
        }

        protected virtual void OnAfterDeserializeEvent()
        {
        }

        float IInternalUse.InternalTotalDepth => TotalDepthLink;
        float IInternalUse.InternalTotalWidth => TotalWidthLink;
        float IInternalUse.InternalTotalHeight => TotalHeightLink;

        protected abstract bool SupportsCategoryLabels { get; }

        protected abstract bool SupportsItemLabels { get; }

        protected abstract bool SupportsGroupLables { get; }

        bool IInternalUse.InternalSupportsCategoryLables => SupportsCategoryLabels;

        bool IInternalUse.InternalSupportsGroupLabels => SupportsGroupLables;

        bool IInternalUse.InternalSupportsItemLabels => SupportsItemLabels;

        #endregion
    }
}