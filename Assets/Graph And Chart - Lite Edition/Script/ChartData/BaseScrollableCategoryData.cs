using UnityEngine;

namespace ChartAndGraph
{
    public class BaseScrollableCategoryData
    {
        public bool Enabled = true;
        public double? MaxX, MaxY, MinX, MinY, MaxRadius;
        public string Name;

        [HideInInspector] public int ViewOrder = -1;
    }
}