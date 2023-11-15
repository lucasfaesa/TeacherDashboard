using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     base class for some chart datasources
    /// </summary>
    [Serializable]
    public abstract class AbstractChartData
    {
        protected List<Slider> mSliders = new();

        protected void RemoveSliderForGroup(string group)
        {
            mSliders.RemoveAll(x => { return x.group == group; });
        }

        protected void RemoveSliderForCategory(string category)
        {
            mSliders.RemoveAll(x => { return x.category == category; });
        }

        protected void RemoveSlider(string category, string group)
        {
            mSliders.RemoveAll(x => { return x.category == category && x.group == group; });
        }

        private bool DoSlider(Slider s)
        {
            return s.UpdateSlider(this);
        }

        protected void UpdateSliders()
        {
            mSliders.RemoveAll(DoSlider);
        }

        protected abstract void SetValueInternal(string column, string row, double value);

        protected class Slider
        {
            public string category;
            public AnimationCurve curve;
            public double from;
            public string group;
            public float startTime;
            public float timeScale = 1f;
            public double to;
            public float totalTime;

            public bool UpdateSlider(AbstractChartData data)
            {
                var time = Time.time;
                var elasped = time - startTime;
                elasped *= timeScale;
                if (elasped > totalTime)
                {
                    data.SetValueInternal(category, group, to);
                    return true;
                }

                var factor = elasped / totalTime;
                if (curve != null)
                    factor = curve.Evaluate(factor);
                var newValue = from * (1.0 - factor) + to * factor;
                data.SetValueInternal(category, group, newValue);
                return false;
            }
        }
    }
}