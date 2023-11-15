using System;

namespace ChartAndGraph
{
    [Serializable]
    public class ChartMainDivisionInfo : ChartDivisionInfo
    {
        public DivisionMessure Messure
        {
            get => messure;
            set
            {
                messure = value;
                RaiseOnChanged();
            }
        }

        public float UnitsPerDivision
        {
            get => unitsPerDivision;
            set
            {
                unitsPerDivision = value;
                RaiseOnChanged();
            }
        }
    }
}