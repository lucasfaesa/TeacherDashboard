using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ChartAndGraph
{
    /// <summary>
    ///     provides functionallity for recieving events for chart items (such as bars and pie slices)
    /// </summary>
    public class ChartItemEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, InternalItemEvents
    {
        /// <summary>
        ///     occures when the mouse is over the item
        /// </summary>
        [Tooltip("Occures when the mouse is over the item")]
        public Event OnMouseHover = new();

        /// <summary>
        ///     occurs when the mouse is no longer over the item
        /// </summary>
        [Tooltip("Occurs when the mouse is no longer over the item")]
        public Event OnMouseLeave = new();

        /// <summary>
        ///     occurs when the user clicks the chart item
        /// </summary>
        [Tooltip("Occurs when the user clicks the chart item")]
        public Event OnSelected = new();

        private bool mMouseDown;

        private bool mMouseOver;

        private IInternalUse mParent;
        private object mUserData;

        private void Start()
        {
        }

        private void OnMouseDown()
        {
            if (mMouseDown == false)
                OnSelected.Invoke(gameObject);
            if (mParent != null)
                mParent.InternalItemSelected(mUserData);
            mMouseDown = true;
        }

        private void OnMouseEnter()
        {
            if (mMouseOver == false)
                OnMouseHover.Invoke(gameObject);
            if (mParent != null)
                mParent.InternalItemHovered(mUserData);
            mMouseOver = true;
        }

        private void OnMouseExit()
        {
            if (mMouseOver)
                OnMouseLeave.Invoke(gameObject);
            if (mParent != null)
                mParent.InternalItemLeave(mUserData);
            mMouseOver = false;
        }

        private void OnMouseUp()
        {
            mMouseDown = false;
        }

        IInternalUse InternalItemEvents.Parent
        {
            get => mParent;

            set => mParent = value;
        }

        object InternalItemEvents.UserData
        {
            get => mUserData;

            set => mUserData = value;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnMouseDown();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnMouseUp();
        }

        [Serializable]
        public class Event : UnityEvent<GameObject>
        {
        }
    }
}