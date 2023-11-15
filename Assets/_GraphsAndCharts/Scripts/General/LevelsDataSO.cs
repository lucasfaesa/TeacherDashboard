using System.Collections.Generic;
using API_Mestrado_Lucas;
using UnityEditor;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.General
{
    [CreateAssetMenu(fileName = "LevelsData", menuName = "ScriptableObjects/LevelsData/LevelsData")]
    public class LevelsDataSO : ScriptableObject
    {
        public List<LevelDTO> LevelsOfThisGroupClass { get; set; }
        public List<QuizDTO> ActiveQuizesOfThisTeacher { get; set; }
        
        public List<LevelDTO> AllLevelsOfGame { get; set; }
        public List<QuizDTO> AllQuizesOfThisTeacher { get; set; }

        public void Reset()
        {
            LevelsOfThisGroupClass = null;
            AllQuizesOfThisTeacher = null;
            ActiveQuizesOfThisTeacher = null;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(LevelsDataSO))]
    public class ButtonEditorLevelsSo : Editor
    {
        private LevelsDataSO instance;

        public override void OnInspectorGUI()
        {
            instance = (LevelsDataSO)target;

            if (GUILayout.Button("Reset SO")) instance.Reset();

            base.OnInspectorGUI();
        }
    }

#endif
}