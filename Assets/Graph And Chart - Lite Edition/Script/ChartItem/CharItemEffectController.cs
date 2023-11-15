using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     manages all the chart effect for a chart item. This includes scale translation and rotation effects
    /// </summary>
    public class CharItemEffectController : MonoBehaviour
    {
        private readonly List<ChartItemEffect> mEffects = new();
        private Vector3 mInitialScale;
        private Transform mParent;

        public CharItemEffectController()
        {
            InitialScale = true;
        }

        internal bool WorkOnParent { get; set; }
        internal bool InitialScale { get; set; }

        protected Transform Parent
        {
            get
            {
                if (mParent == null)
                    mParent = transform.parent;
                return mParent;
            }
        }

        private void Start()
        {
            mInitialScale = transform.localScale;
        }

        private void Update()
        {
            var trans = transform;
            if (WorkOnParent)
            {
                trans = Parent;
                if (trans == null)
                    return;
            }

            var scale = new Vector3(1f, 1f, 1f);
            if (InitialScale)
                scale = mInitialScale;
            var translation = Vector3.zero;
            var rotation = Quaternion.identity;

            for (var i = 0; i < mEffects.Count; i++)
            {
                var effect = mEffects[i];
                if (effect == null || effect.gameObject == null)
                {
                    mEffects.RemoveAt(i);
                    --i;
                    continue;
                }

                scale.x *= effect.ScaleMultiplier.x;
                scale.y *= effect.ScaleMultiplier.y;
                scale.z *= effect.ScaleMultiplier.z;

                translation += effect.Translation;
                rotation *= effect.Rotation;
            }

            trans.localScale = scale;
        }

        private void OnTransformParentChanged()
        {
            mInitialScale = transform.localScale;
        }

        public void Unregister(ChartItemEffect effect)
        {
            mEffects.Remove(effect);
            if (mEffects.Count == 0)
                enabled = false;
        }

        public void Register(ChartItemEffect effect)
        {
            if (mEffects.Contains(effect))
                return;
            if (enabled == false)
                enabled = true;
            mEffects.Add(effect);
        }
    }
}