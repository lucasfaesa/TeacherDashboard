using System;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     base class for all chart item effects
    /// </summary>
    public abstract class ChartItemEffect : MonoBehaviour
    {
        private CharItemEffectController mController;
        internal int ItemIndex { get; set; }
        internal int ItemType { get; set; }
        internal object ItemData { get; set; }

        protected CharItemEffectController Controller
        {
            get
            {
                if (mController == null)
                {
                    mController = GetComponent<CharItemEffectController>();
                    if (mController == null)
                        mController = gameObject.AddComponent<CharItemEffectController>();
                }

                return mController;
            }
        }

        /// <summary>
        ///     applies a scaling to the object
        /// </summary>
        internal abstract Vector3 ScaleMultiplier { get; }

        /// <summary>
        ///     applies rotation to the object
        /// </summary>
        internal abstract Quaternion Rotation { get; }

        /// <summary>
        ///     applies translation to the object
        /// </summary>
        internal abstract Vector3 Translation { get; }

        protected virtual void Start()
        {
            Register();
        }

        protected virtual void OnEnable()
        {
            Register();
        }

        protected virtual void OnDisable()
        {
            Unregister();
        }

        public event Action<ChartItemEffect> Deactivate;

        protected void RaiseDeactivated()
        {
            if (Deactivate != null)
                Deactivate(this);
        }

        private void Register()
        {
            var control = Controller;
            if (control != null)
                control.Register(this);
        }

        private void Unregister()
        {
            var control = Controller;
            if (control != null)
                control.Unregister(this);
        }

        protected virtual void Destroy()
        {
            Unregister();
        }

        public abstract void TriggerIn(bool deactivateOnEnd);

        public abstract void TriggerOut(bool deactivateOnEnd);
    }
}