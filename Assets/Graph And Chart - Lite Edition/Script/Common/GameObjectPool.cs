using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     pools gameobjects for repeated use
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GameObjectPool<T> where T : MonoBehaviour
    {
        private readonly List<T> mPool = new();

        public void RecycleObject(T obj)
        {
            mPool.Add(obj);
        }

        public T TakeObject()
        {
            if (mPool.Count == 0)
                return default;
            var last = mPool.Count - 1;
            var res = mPool[last];
            mPool.RemoveAt(last);
            if (res.gameObject.activeInHierarchy == false)
                res.gameObject.SetActive(true);
            return res;
        }

        public void DestoryAll()
        {
            for (var i = 0; i < mPool.Count; i++)
            {
                var t = mPool[i];
                if (t != null && t.gameObject != null) ChartCommon.SafeDestroy(t.gameObject);
            }

            mPool.Clear();
        }

        public void DeactivateObjects()
        {
            for (var i = 0; i < mPool.Count; i++)
            {
                var t = mPool[i];
                if (t != null && t.gameObject != null)
                    if (t.gameObject.activeInHierarchy)
                        t.gameObject.SetActive(false);
            }
        }
    }
}