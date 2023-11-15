using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Inside.Flow
{
    public class ActivityLooped : MonoBehaviour
    {
        public UnityEvent execute;
        public float loopInterval;

        private WaitForSeconds loopWait;

        public virtual void Execute()
        {
            //Debug.Log($"Activity on '{this.name}' executing.");

            loopWait = new WaitForSeconds(loopInterval);

            StopAllCoroutines();
            StartCoroutine(LoopExecution());
        }

        private IEnumerator LoopExecution()
        {
            while (true)
            {
                execute.Invoke();
                yield return loopWait;
            }
        }
    }


#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [CustomEditor(typeof(ActivityLooped))]
    public class LoopedActivityEditor : Editor
    {
        //-------------------------------------------------
        // Custom Inspector GUI allows us to click from within the UI
        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var activity = (ActivityLooped)target;

            if (GUILayout.Button("Trigger Execute")) activity.Execute();
        }
    }
#endif
}