using System;
using System.Collections.Generic;
using API_Mestrado_Lucas;
using UnityEngine;

namespace _TeacherDashboard.Scripts
{
    [CreateAssetMenu(fileName = "Questions", menuName = "ScriptableObjects/Questions/QuestionsSO")]
    public class QuizesSO : ScriptableObject
    {
        public List<QuizDTO> Quizes { get; set; }

        public event Action UpdateQuizesData;

        public void UpdateQuizes()
        {
            UpdateQuizesData?.Invoke();
        }
    }
}