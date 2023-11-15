using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    internal class ChartItemTextBlend : ChartItemLerpEffect
    {
        private readonly Dictionary<Object, float> mInitialValues = new();
        private CanvasRenderer mRenderer;
        private Shadow[] mShadows;

        private Text mText;

        internal override Quaternion Rotation => Quaternion.identity;

        internal override Vector3 ScaleMultiplier => new Vector3(1f, 1f, 1f);

        internal override Vector3 Translation => Vector3.zero;

        protected override void Start()
        {
            base.Start();
            mText = GetComponent<Text>();
            mShadows = GetComponents<Shadow>();
            foreach (var s in mShadows)
                mInitialValues.Add(s, s.effectColor.a);
            ApplyLerp(0f);
        }

        protected override float GetStartValue()
        {
            if (mText != null)
                return mText.color.a;
            return 0f;
        }

        private CanvasRenderer EnsureRenderer()
        {
            if (mRenderer == null)
                mRenderer = GetComponent<CanvasRenderer>();
            return mRenderer;
        }

        protected override void ApplyLerp(float value)
        {
            for (var i = 0; i < mShadows.Length; i++)
            {
                var s = mShadows[i];
                float inital;
                if (mInitialValues.TryGetValue(s, out inital) == false)
                    continue;
                var c = s.effectColor;
                c.a = Mathf.Lerp(0f, inital, value);
                s.effectColor = c;
            }

            if (mText != null)
            {
                var c = mText.color;
                c.a = Mathf.Clamp(value, 0f, 1f);
                mText.color = c;
                var rend = EnsureRenderer();
                if (rend != null)
                {
                    if (value <= 0f)
                    {
                        if (rend.cull == false)
                            rend.cull = true;
                    }
                    else
                    {
                        if (rend.cull)
                            rend.cull = false;
                    }
                }
            }
        }
    }
}