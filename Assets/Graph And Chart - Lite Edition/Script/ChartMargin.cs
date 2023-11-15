using System;
using UnityEngine;

namespace ChartAndGraph
{
    [Serializable]
    public struct ChartMagin
    {
        [SerializeField] private float left, top, right, bottom;

        public ChartMagin(float leftMargin, float topMargin, float rightMargin, float bottomMargin)
        {
            left = leftMargin;
            right = rightMargin;
            top = topMargin;
            bottom = bottomMargin;
        }

        public float Left => left;
        public float Right => right;
        public float Top => top;
        public float Bottom => bottom;
    }
}