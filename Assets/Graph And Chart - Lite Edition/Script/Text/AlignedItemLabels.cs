using UnityEngine;

namespace ChartAndGraph
{
    public abstract class AlignedItemLabels : ItemLabelsBase
    {
        /// <summary>
        ///     Select the alignment of the label relative to the item
        /// </summary>
        [SerializeField] [Tooltip("Select the alignment of the label relative to the item")]
        private ChartLabelAlignment alignment;

        /// <summary>
        ///     Select the alignment of the label relative to the item
        /// </summary>
        public ChartLabelAlignment Alignment
        {
            get => alignment;
            set
            {
                alignment = value;
                RaiseOnUpdate();
            }
        }
    }
}