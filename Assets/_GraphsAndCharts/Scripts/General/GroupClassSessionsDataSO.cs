using System;
using System.Collections.Generic;
using API_Mestrado_Lucas;
using UnityEditor;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.General
{
    [CreateAssetMenu(fileName = "GroupClassSessionsData",
        menuName = "ScriptableObjects/SessionsData/GroupClassSessionsData")]
    public class GroupClassSessionsDataSO : ScriptableObject
    {
        public List<CompleteStudentSessionDTO> CompleteGroupClassSession { get; set; }

        public void Reset()
        {
            CompleteGroupClassSession = null;
        }

        public event Action OnNewSessionsGot;
        public event Action OnNewLevelsGot;

        public event Action OnSessionsAndLevelsGot;

        public void GotSessions()
        {
            OnNewSessionsGot?.Invoke();
        }

        public void GotLevels()
        {
            OnNewLevelsGot?.Invoke();
        }

        public void GotSessionsAndLevels()
        {
            OnSessionsAndLevelsGot?.Invoke();
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(GroupClassSessionsDataSO))]
    public class ButtonEditor : Editor
    {
        private GroupClassSessionsDataSO instance;

        public override void OnInspectorGUI()
        {
            instance = (GroupClassSessionsDataSO)target;

            if (GUILayout.Button("Reset SO")) instance.Reset();

            base.OnInspectorGUI();
        }
    }

#endif
}