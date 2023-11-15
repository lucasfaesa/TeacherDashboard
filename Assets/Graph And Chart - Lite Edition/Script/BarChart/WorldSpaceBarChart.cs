using UnityEngine;

namespace ChartAndGraph
{
    public class WorldSpaceBarChart : BarChart
    {
        /// <summary>
        ///     If this value is set all the text in the chart will be rendered to this specific camera. otherwise text is rendered
        ///     to the main camera
        /// </summary>
        [SerializeField]
        [Tooltip(
            "If this value is set all the text in the chart will be rendered to this specific camera. otherwise text is rendered to the main camera")]
        private Camera textCamera;

        /// <summary>
        ///     The distance from the camera at which the text is at it's original size.
        /// </summary>
        [SerializeField] [Tooltip("The distance from the camera at which the text is at it's original size")]
        private float textIdleDistance = 20f;

        /// <summary>
        ///     prefab for all the bar elements of the chart. must be one unit size and with a bottom middle pivot
        /// </summary>
        [SerializeField]
        [Tooltip("prefab for all the bar elements of the chart. must be one unit size and with a bottom middle pivot")]
        private GameObject barPrefab;

        /// <summary>
        ///     The seperation between the axis and the chart bars.
        /// </summary>
        [SerializeField] [Tooltip("The seperation between the axis and the chart bars")]
        private ChartOrientedSize AxisSeperation = new();

        /// <summary>
        ///     The seperation between bars of the same group.
        ///     Use cases:
        ///     1. set the depth to 0 to make each group look more 2d.
        ///     2. set the breadth to 0 to make align the bars of each group along the z axis
        /// </summary>
        [SerializeField] [Tooltip("The seperation between bars of the same group")]
        private ChartOrientedSize barSeperation = new();

        /// <summary>
        ///     The seperation between bar groups.
        ///     Use cases:
        ///     1.set the depth to 0 for a more 2d look.
        ///     2.set the breadth to 0 to align the groups on the z axis
        /// </summary>
        [SerializeField] [Tooltip("seperation between bar groups")]
        private ChartOrientedSize groupSeperation = new();

        /// <summary>
        ///     the size of each bar in the chart
        /// </summary>
        [SerializeField] [Tooltip("the size of each bar in the chart")]
        private ChartOrientedSize barSize = new(1f, 1f);

        /// <summary>
        ///     If this value is set all the text in the chart will be rendered to this specific camera. otherwise text is rendered
        ///     to the main camera
        /// </summary>
        public Camera TextCamera
        {
            get => textCamera;
            set
            {
                textCamera = value;
                OnPropertyUpdated();
            }
        }

        public override bool IsCanvas => false;

        /// <summary>
        ///     The distance from the camera at which the text is at it's original size.
        /// </summary>
        public float TextIdleDistance
        {
            get => textIdleDistance;
            set
            {
                textIdleDistance = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     prefab for all the bar elements of the chart. must be one unit size and with a bottom middle pivot
        /// </summary>
        public GameObject BarPrefab
        {
            get => barPrefab;
            set
            {
                barPrefab = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     The seperation between the axis and the chart bars
        /// </summary>
        public ChartOrientedSize axisSeperation
        {
            get => AxisSeperation;
            set
            {
                AxisSeperation = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     The seperation between bars of the same group.
        ///     Use cases:
        ///     1. set the depth to 0 to make each group look more 2d.
        ///     2. set the breadth to 0 to make align the bars of each group along the z axis
        /// </summary>
        public ChartOrientedSize BarSeperation
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
        ///     Use cases:
        ///     1.set the depth to 0 for a more 2d look.
        ///     2.set the breadth to 0 to align the groups on the z axis
        ///     <summary>
        public ChartOrientedSize GroupSeperation
        {
            get => groupSeperation;
            set
            {
                groupSeperation = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        ///     the size of each bar in the chart
        /// </summary>
        public ChartOrientedSize BarSize
        {
            get => barSize;
            set
            {
                barSize = value;
                OnPropertyUpdated();
            }
        }

        protected override ChartOrientedSize AxisSeperationLink => AxisSeperation;

        protected override ChartOrientedSize BarSeperationLink => BarSeperation;

        protected override ChartOrientedSize GroupSeperationLink => GroupSeperation;

        protected override ChartOrientedSize BarSizeLink => BarSize;

        protected override Camera TextCameraLink => TextCamera;

        protected override float TextIdleDistanceLink => TextIdleDistance;

        protected override GameObject BarPrefabLink => BarPrefab;

        protected override void ValidateProperties()
        {
            base.ValidateProperties();
            if (barSize.Breadth < 0f)
                barSize.Breadth = 0f;
            if (barSize.Depth < 0f)
                barSize.Depth = 0f;
        }
    }
}