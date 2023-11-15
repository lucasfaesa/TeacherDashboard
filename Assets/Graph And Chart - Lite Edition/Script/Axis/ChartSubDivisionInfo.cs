using System;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     holds settings for an axis division.
    /// </summary>
    [Serializable]
    internal class ChartSubDivisionInfo : ChartDivisionInfo
    {
        protected override float ValidateTotal(float total)
        {
            return Mathf.Round(total);
        }
    }
}