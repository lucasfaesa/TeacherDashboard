using System;
using UnityEngine;

namespace ChartAndGraph
{
    public class ItemLabels : AlignedItemLabels
    {
        /// <summary>
        ///     the number of fraction digits in the text labels
        /// </summary>
        [Range(0, 7)] [SerializeField] private int fractionDigits;

        /// <summary>
        ///     the number of fraction digits in the text labels
        /// </summary>
        public int FractionDigits
        {
            get => fractionDigits;
            set
            {
                fractionDigits = value;
                RaiseOnUpdate();
            }
        }

        protected override Action<IInternalUse, bool> Assign
        {
            get
            {
                return (x, clear) =>
                {
                    if (clear)
                    {
                        if (x.ItemLabels == this)
                            x.ItemLabels = null;
                    }
                    else
                    {
                        if (x.ItemLabels != this)
                            x.ItemLabels = this;
                    }
                };
            }
        }
    }
}