﻿using UnityEngine;

namespace ChartAndGraph
{
    public abstract class BarGenerator : MonoBehaviour, IBarGenerator
    {
        /// <summary>
        ///     generates a bar within the specified rect. The rect is in local coordinates
        /// </summary>
        /// <param name="rect">A rect that specifies the bounds of the bar. the rect is in local coordinates</param>
        /// <param name="normalizedSize">a value between 0 to 1 representing the size of the rect relative to the size of the chart</param>
        public abstract void Generate(float normalizedSize, float scale);

        /// <summary>
        ///     clears the bar the was generated be the Generate method.
        /// </summary>
        public abstract void Clear();
    }
}