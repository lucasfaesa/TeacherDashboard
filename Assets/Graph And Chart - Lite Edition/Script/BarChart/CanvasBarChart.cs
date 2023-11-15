#define Graph_And_Chart_PRO
using UnityEngine;

namespace ChartAndGraph
{
    public class CanvasBarChart : BarChart, ICanvas
    {
        /// <summary>
        ///     The seperation between the axis and the chart bars.
        /// </summary>
        [SerializeField] private bool fitToContainer;

        [SerializeField] private ChartMagin fitMargin;

        [SerializeField]
        /// <summary>
        /// prefab for the bar elements of the chart. must be the size of one unit with a pivot at the middle bottom
        /// </summary>
        [Tooltip(
            "Prefab for the bar elements of the chart. must be the size of one unit with a pivot at the middle bottom")]
        private CanvasRenderer barPrefab;

        /// <summary>
        ///     The seperation between the axis and the chart bars.
        /// </summary>
        [SerializeField] [Tooltip("The seperation between the axis and the chart bars")]
        private float axisSeperation = 20f;

        /// <summary>
        ///     seperation between bar of the same group
        /// </summary>
        [SerializeField] [Tooltip("seperation between bar of the same group")]
        private float barSeperation = 45f;

        /// <summary>
        ///     seperation between bar groups
        /// </summary>
        [SerializeField] [Tooltip("seperation between bar groups")]
        private float groupSeperation = 220f;

        /// <summary>
        ///     the width of each bar in the chart
        /// </summary>
        [SerializeField] [Tooltip("the width of each bar in the chart")]
        private float barSize = 20f;
        //protected override Vector3 CanvasFitOffset
        //{
        //    get
        //    {
        //        return new Vector3();
        //    }
        //}

        /// <summary>
        /// </summary>
        [SerializeField] [Tooltip("")] private FitOrientation barFitDirection = FitOrientation.Normal;

        [SerializeField] [Tooltip("")] private FitType barFitType = FitType.Aspect;


        /// <summary>
        /// </summary>
        [SerializeField] [Tooltip("")] private FitAlign barFitAlign = FitAlign.CenterXCenterY;

        public bool FitToContainer
        {
            get => fitToContainer;
            set
            {
                fitToContainer = value;
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

        protected override ChartMagin MarginLink => fitMargin;

        /// <summary>
        ///     prefab for the bar elements of the chart. must be the size of one unit with a pivot at the middle bottom
        /// </summary>
        public CanvasRenderer BarPrefab
        {
            get => barPrefab;
            set
            {
                barPrefab = value;
                OnPropertyUpdated();
            }
        }

        public override bool IsCanvas => true;

        protected override float TotalDepthLink => 0.0f;

        /// <summary>
        ///     The seperation between the axis and the chart bars.
        /// </summary>
        public float AxisSeperation
        {
            get => axisSeperation;
            set
            {
                axisSeperation = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     seperation between bars of the same group.
        /// </summary>
        public float BarSeperation
        {
            get => barSeperation;
            set
            {
                barSeperation = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     The seperation between bar groups.
        ///     <summary>
        public float GroupSeperation
        {
            get => groupSeperation;
            set
            {
                groupSeperation = value;
                OnPropertyUpdated();
            }
        }

        public override bool SupportRealtimeGeneration => false;

        /// <summary>
        ///     the width of each bar in the chart
        /// </summary>
        public float BarSize
        {
            get => barSize;
            set
            {
                barSize = value;
                OnPropertyUpdated();
            }
        }

        protected override ChartOrientedSize AxisSeperationLink => new ChartOrientedSize(AxisSeperation);

        protected override ChartOrientedSize BarSeperationLink => new ChartOrientedSize(BarSeperation);

        protected override ChartOrientedSize GroupSeperationLink => new ChartOrientedSize(GroupSeperation);

        protected override ChartOrientedSize BarSizeLink => new ChartOrientedSize(BarSize);

        /// <summary>
        ///     the width of each bar in the chart
        /// </summary
        public FitOrientation BarFitDirection
        {
            get => barFitDirection;
            set
            {
                barFitDirection = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     the width of each bar in the chart
        /// </summary
        public FitType BarFitType
        {
            get => barFitType;
            set
            {
                barFitType = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     the width of each bar in the chart
        /// </summary
        public FitAlign BarFitAlign
        {
            get => barFitAlign;
            set
            {
                barFitAlign = value;
                OnPropertyUpdated();
            }
        }

        protected override float FitZRotationCanvas => barFitDirection == FitOrientation.Vertical ? -90 :
            barFitDirection == FitOrientation.VerticalOpopsite ? 90 : 0;

        protected override FitType FitAspectCanvas => BarFitType;
        protected override FitAlign FitAlignCanvas => BarFitAlign;

        protected override GameObject BarPrefabLink
        {
            get
            {
                if (BarPrefab == null)
                    return null;
                return BarPrefab.gameObject;
            }
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void SetBarSize(GameObject bar, Vector3 size, float elevation)
        {
            var rect = bar.GetComponent<RectTransform>();

            if (rect != null)
            {
                var ySize = size.y;
                var yAnchor = 0f;
                if (ySize < 0)
                {
                    ySize = -ySize;
                    yAnchor = 1;
                }

                rect.pivot = new Vector2(0.5f, yAnchor);
                rect.sizeDelta = new Vector2(size.x, ySize);
                Vector2 v = rect.localPosition;
                v.y = elevation;
                rect.localPosition = v;
            }
            else
            {
                base.SetBarSize(bar, size, elevation);
            }
        }

        [ContextMenu("Refresh chart")]
        public override void InternalGenerateChart()
        {
            var trans = GetComponent<RectTransform>();

            if (FitToContainer)
            {
                if (barFitDirection == FitOrientation.Normal)
                {
                    var width = MessureWidth();
                    heightRatio = width * (trans.rect.size.y / trans.rect.size.x);
                }
                else
                {
                    var width = MessureWidth();
                    heightRatio = width * (trans.rect.size.x / trans.rect.size.y);
                }
            }

            base.InternalGenerateChart();
            //if (TextController != null && TextController.gameObject)
            //    TextController.gameObject.transform.SetAsLastSibling();
            //float widthScale = trans.rect.size.x / TotalWidth;
            //float heightScale = trans.rect.size.y / HeightRatio;
            //GameObject fixPosition = new GameObject();
            //ChartCommon.HideObject(fixPosition, hideHierarchy);
            //fixPosition.AddComponent<ChartItem>();
            //fixPosition.transform.position = transform.position;
            //while (gameObject.transform.childCount > 0)
            //    transform.GetChild(0).SetParent(fixPosition.transform, false);
            //fixPosition.transform.SetParent(transform, false);
            //fixPosition.transform.localScale = new Vector3(1f, 1f, 1f);
            //float uniformScale = Math.Min(widthScale, heightScale);
            //fixPosition.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
            //fixPosition.transform.localPosition = new Vector3(-TotalWidth * uniformScale * 0.5f, -HeightRatio * uniformScale * 0.5f, 0f);
        }
    }
}