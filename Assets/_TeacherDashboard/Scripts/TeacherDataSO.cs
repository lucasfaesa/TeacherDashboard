using System;
using System.Collections.Generic;
using API_Mestrado_Lucas;
using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    [CreateAssetMenu(fileName = "TeacherData", menuName = "ScriptableObjects/TeacherData")]
    public class TeacherDataSO : ScriptableObject
    {
        [SerializeField] private bool useDebug;
        public TeacherDTO TeacherDto { get; set; }
        public int? CurrentlyChosenGroupClassId { get; set; }

        
        public bool UseDebug
        {
            get
            {
                #if UNITY_EDITOR
                    return useDebug;
                #else
                    return false;
                #endif  
            } 
            set => useDebug = value;
        }

        [field: SerializeField] public DebugTeacherData DebugData { get; set; }

        public bool LoggedIn { get; set; }
        
        public void Reset()
        {
            TeacherDto = null;
            CurrentlyChosenGroupClassId = null;
        }

        private void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        public event Action CurrentGroupClassChanged;

        public void ChangeCurrentGroupClass(int id)
        {
            CurrentlyChosenGroupClassId = id;
            CurrentGroupClassChanged?.Invoke();
        }
    }

    [Serializable]
    public class DebugTeacherData
    {
        public int teacherId;
        public int currentlyChosenGroupClass;
        public List<int> groupClassIds;
    }
}