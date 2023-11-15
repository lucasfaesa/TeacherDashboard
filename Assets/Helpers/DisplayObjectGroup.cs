using System;
using UnityEngine;

namespace Inside
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DisplayObjectGroup : DisplayObject
    {
        private CanvasGroup _canvasGroup;

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }

        public void SoloInteractable()
        {
            ForEachSibling(cg => { cg.interactable = false; });

            CanvasGroup.interactable = true;
        }

        public void SetAllInteractable(bool state)
        {
            ForEachSibling(cg => { cg.interactable = state; });

            CanvasGroup.interactable = state;
        }

        protected virtual void ForEachSibling(Action<CanvasGroup> action)
        {
            foreach (Transform s in transform.parent)
            {
                if (s == transform) continue;
                if (s.GetComponent<DisplayObjectGroup>() == null) continue;

                action.Invoke(s.GetComponent<CanvasGroup>());
            }
        }
    }
}