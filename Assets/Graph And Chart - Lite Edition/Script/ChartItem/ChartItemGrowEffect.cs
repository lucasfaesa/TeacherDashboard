using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     makes the chart item grow and shrink with easing. this can be connected to hover events for example
    /// </summary>
    internal class ChartItemGrowEffect : ChartItemEffect
    {
        private const int NoOp = 0;
        private const int GrowOp = 1;
        private const int ShrinkOp = 2;
        private const int GrowShrinkOp = 3;
        public float GrowMultiplier = 1.2f;

        public bool VerticalOnly;
        public bool HorizontalOnly;

        /// <summary>
        ///     scales the time used in the easing curves
        /// </summary>
        public float TimeScale = 1f;

        /// <summary>
        ///     easing function for the grow effect. when the curve is at 0 there will be no change , when the curve is at 1 the
        ///     change will be equal to the GrowMultiplier property
        /// </summary>
        public AnimationCurve GrowEaseFunction = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        ///     easing function for the shrink effect. when the curve is at 0 there will be no change , when the curve is at 1 the
        ///     change will be equal to the GrowMultiplier property
        /// </summary>
        public AnimationCurve ShrinkEaseFunction = AnimationCurve.EaseInOut(1f, 1f, 0f, 0f);

        private bool mDeactivateOnEnd;

        private float mScaleMultiplier = 1f;
        private float mStartTime;
        private float mStartValue;
        private int Operation = NoOp;

        internal override Vector3 ScaleMultiplier
        {
            get
            {
                if (VerticalOnly && !HorizontalOnly)
                    return new Vector3(1f, mScaleMultiplier, 1f);
                if (HorizontalOnly && !VerticalOnly)
                    return new Vector3(mScaleMultiplier, 1f, 1f);
                return new Vector3(mScaleMultiplier, mScaleMultiplier, mScaleMultiplier);
            }
        }

        internal override Quaternion Rotation => Quaternion.identity;

        internal override Vector3 Translation => Vector3.zero;

        private void Update()
        {
            var opTime = Time.time - mStartTime;
            opTime *= TimeScale;
            float val;
            switch (Operation)
            {
                case GrowOp:
                    FixEaseFunction(GrowEaseFunction);
                    val = GrowEaseFunction.Evaluate(opTime);
                    mScaleMultiplier = ChartCommon.SmoothLerp(mStartValue, GrowMultiplier, val);
                    if (CheckAnimationEnded(opTime, GrowEaseFunction))
                    {
                        Operation = NoOp;
                        mScaleMultiplier = GrowMultiplier;
                    }

                    break;
                case ShrinkOp:
                    FixEaseFunction(ShrinkEaseFunction);
                    val = ShrinkEaseFunction.Evaluate(opTime);
                    mScaleMultiplier = ChartCommon.SmoothLerp(mStartValue, 1f, val);
                    if (CheckAnimationEnded(opTime, ShrinkEaseFunction))
                    {
                        Operation = NoOp;
                        mScaleMultiplier = 1f;
                    }

                    break;
                case GrowShrinkOp:
                    FixEaseFunction(GrowEaseFunction);
                    val = GrowEaseFunction.Evaluate(opTime);
                    mScaleMultiplier = ChartCommon.SmoothLerp(mStartValue, GrowMultiplier, val);
                    if (CheckAnimationEnded(opTime, GrowEaseFunction))
                    {
                        mScaleMultiplier = GrowMultiplier;
                        Shrink();
                    }

                    break;
            }
        }

        /// <summary>
        ///     equivalent to calling Grow and Shrink one after the other
        /// </summary>
        public void GrowAndShrink()
        {
            mStartTime = Time.time;
            mStartValue = mScaleMultiplier;
            Operation = GrowShrinkOp;
            enabled = true;
        }

        public bool CheckAnimationEnded(float time, AnimationCurve curve)
        {
            if (curve.length == 0)
                return true;
            var ended = time > curve.keys[curve.length - 1].time;
            if (ended)
                if (mDeactivateOnEnd)
                {
                    RaiseDeactivated();
                    enabled = false;
                    gameObject.SetActive(false);
                    mDeactivateOnEnd = false;
                }

            return ended;
        }

        private void FixEaseFunction(AnimationCurve curve)
        {
            curve.postWrapMode = WrapMode.Once;
            curve.preWrapMode = WrapMode.Once;
        }

        public override void TriggerOut(bool deactivateOnEnd)
        {
            mDeactivateOnEnd = deactivateOnEnd;
            Shrink();
        }

        public override void TriggerIn(bool deactivateOnEnd)
        {
            mDeactivateOnEnd = deactivateOnEnd;
            Grow();
        }

        /// <summary>
        ///     Grows the size of the object
        /// </summary>
        public void Grow()
        {
            mStartTime = Time.time;
            mStartValue = mScaleMultiplier;
            Operation = GrowOp;
            enabled = true;
        }

        /// <summary>
        ///     Shrinks the object back to the original size
        /// </summary>
        public void Shrink()
        {
            mStartTime = Time.time;
            mStartValue = mScaleMultiplier;
            Operation = ShrinkOp;
            enabled = true;
        }
    }
}