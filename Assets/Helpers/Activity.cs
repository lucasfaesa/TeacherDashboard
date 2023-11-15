using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Inside.Flow
{
    public class Activity : MonoBehaviour
    {
        [SerializeField] private UnityEvent execute;

        public virtual void Execute()
        {
            //Debug.Log($"Activity on '{this.name}' executed.");

            execute.Invoke();
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Activity))]
    public class ActivityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var activity = (Activity)target;

            if (GUILayout.Button("Execute")) activity.Execute();
        }
    }
#endif
}