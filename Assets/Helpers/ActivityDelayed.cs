using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Inside.Flow
{
    public class ActivityDelayed : Activity
    {
        [SerializeField] private float waitTime;

        private WaitForSeconds wait;

        public override void Execute()
        {
            StartCoroutine(WaitAndThenExecute());
        }

        private IEnumerator WaitAndThenExecute()
        {
            if (wait == null) wait = new WaitForSeconds(waitTime);

            yield return wait;

            base.Execute();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ActivityDelayed))]
    public class DelayedActivityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var activity = (ActivityDelayed)target;

            if (GUILayout.Button("Trigger Execute")) activity.Execute();
        }
    }
#endif
}