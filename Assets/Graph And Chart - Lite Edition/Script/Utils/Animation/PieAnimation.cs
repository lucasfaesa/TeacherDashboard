using UnityEngine;

namespace ChartAndGraph
{
    public class PieAnimation : MonoBehaviour
    {
        public bool AnimateOnStart = true;
        public bool AnimateOnEnable = true;

        [Range(0f, 360f)] public float AnimateSpan = 360f;

        public float AnimationTime = 2f;
        public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        private PieChart pieChart;
        private float StartTime = -1f;

        private void Start()
        {
            pieChart = GetComponent<PieChart>();
            if (AnimateOnStart)
                Animate();
        }

        private void Update()
        {
            if (StartTime >= 0f && pieChart != null)
            {
                var curveTime = 0f;
                if (Curve.length > 0)
                    curveTime = Curve.keys[Curve.length - 1].time;
                var elasped = Time.time - StartTime;
                elasped /= AnimationTime;
                var blend = elasped;
                elasped *= curveTime;
                elasped = Curve.Evaluate(elasped);
                if (blend >= 1f)
                {
                    pieChart.AngleSpan = AnimateSpan;
                    StartTime = -1f;
                }
                else
                {
                    pieChart.AngleSpan = Mathf.Lerp(0f, AnimateSpan, elasped);
                }
            }
        }

        public void OnEnable()
        {
            pieChart = GetComponent<PieChart>();
            if (AnimateOnEnable)
                Animate();
        }

        public void Animate()
        {
            if (pieChart != null)
            {
                if (StartTime < 0f)
                    AnimateSpan = pieChart.AngleSpan;
                pieChart.AngleSpan = Curve.Evaluate(0f);
                StartTime = Time.time;
            }
        }
    }
}