using System;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     base class for all chart setting items
    /// </summary>
    [RequireComponent(typeof(AnyChart))]
    [ExecuteInEditMode]
    public abstract class ChartSettingItemBase : MonoBehaviour, IInternalSettings
    {
        private AnyChart mChart;

        protected abstract Action<IInternalUse, bool> Assign { get; }

        private AnyChart SafeChart
        {
            get
            {
                if (mChart == null)
                    mChart = GetComponent<AnyChart>();
                return mChart;
            }
        }

        protected virtual void Start()
        {
            SafeAssign(false);
        }

        protected virtual void OnEnable()
        {
            SafeAssign(false);
        }

        protected virtual void OnDisable()
        {
            SafeAssign(true);
        }

        protected virtual void OnDestroy()
        {
            SafeAssign(true);
        }

        protected virtual void OnValidate()
        {
            var chart = SafeChart;
            if (chart != null)
                ((IInternalUse)chart).CallOnValidate();
        }

        private event EventHandler OnDataUpdate;
        private event EventHandler OnDataChanged;

        protected void AddInnerItem(IInternalSettings item)
        {
            item.InternalOnDataChanged += Item_InternalOnDataChanged;
            item.InternalOnDataUpdate += Item_InternalOnDataUpdate;
        }

        private void Item_InternalOnDataUpdate(object sender, EventArgs e)
        {
            RaiseOnUpdate();
        }

        private void Item_InternalOnDataChanged(object sender, EventArgs e)
        {
            RaiseOnChanged();
        }

        protected virtual void RaiseOnChanged()
        {
            if (OnDataChanged != null)
                OnDataChanged(this, EventArgs.Empty);
        }

        protected virtual void RaiseOnUpdate()
        {
            if (OnDataUpdate != null)
                OnDataUpdate(this, EventArgs.Empty);
        }

        private void SafeAssign(bool clear)
        {
            var chart = SafeChart;
            if (chart != null)
                Assign(chart, clear);
        }

        #region Intenal Use

        event EventHandler IInternalSettings.InternalOnDataUpdate
        {
            add => OnDataUpdate += value;

            remove => OnDataUpdate -= value;
        }

        event EventHandler IInternalSettings.InternalOnDataChanged
        {
            add => OnDataChanged += value;
            remove => OnDataChanged -= value;
        }

        #endregion
    }
}