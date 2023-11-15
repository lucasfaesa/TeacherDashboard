using System;
using UnityEngine;

namespace Inside
{
    public class DisplayObject : MonoBehaviour
    {
        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }

        public void Solo()
        {
            ForEachSibling(t => { t.gameObject.SetActive(false); });

            gameObject.SetActive(true);
        }

        public void HideAll()
        {
            ForEachSibling(t => { t.gameObject.SetActive(false); });

            gameObject.SetActive(false);
        }

        protected virtual void ForEachSibling(Action<Transform> action)
        {
            foreach (Transform s in transform.parent)
            {
                if (s == transform) continue;
                if (s.GetComponent<DisplayObject>() == null) continue;

                action.Invoke(s);
            }
        }
    }
}