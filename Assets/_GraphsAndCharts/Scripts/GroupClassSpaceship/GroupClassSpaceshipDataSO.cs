using System;
using System.Collections.Generic;
using API_Mestrado_Lucas;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.GroupClassSpaceship
{
    [CreateAssetMenu(fileName = "GroupClassSpaceshipData", menuName = "ScriptableObjects/SessionsData/GroupClassSpaceshipData")]
    public class GroupClassSpaceshipDataSO : ScriptableObject
    {
        public List<StudentWrongAnswersDTO> AllStudentWrongAnswersOfQuiz { get; set; } = new();
        public List<QuizDTO> AllQuizesOfTeacher { get; set; } = new();
        
        public QuizDTO CurrentSelectedQuiz { get; set; }
        
        public event Action insufficientData;
        public event Action<QuizDTO> quizSelected;
        public event Action allQuizesOfTeacherGot;
        public event Action studentWrongAnswersGot;
        
        public void InsufficientData()
        {
            insufficientData?.Invoke();
        }

        public void QuizSelected(QuizDTO quizDto)
        {
            CurrentSelectedQuiz = quizDto;
            quizSelected?.Invoke(quizDto);
        }

        public void AllQuizesOfTeacherGot()
        {
            allQuizesOfTeacherGot?.Invoke();
        }

        public void StudentWrongAnswersGot()
        {
            studentWrongAnswersGot?.Invoke();
        }
        
        public void Reset()
        {
            AllStudentWrongAnswersOfQuiz = new();
            AllQuizesOfTeacher = new();
            CurrentSelectedQuiz = null;
        }
    }
}
