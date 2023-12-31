﻿using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     lerps the material changes of the chart item. This is used to achive smooth transition between the chart item
    ///     colors.
    /// </summary>
    internal class ChartItemMaterialLerpEffect : MonoBehaviour
    {
        /// <summary>
        ///     the speed of the interpolation between the materials
        /// </summary>
        public float LerpTime = 1f;
    }
}