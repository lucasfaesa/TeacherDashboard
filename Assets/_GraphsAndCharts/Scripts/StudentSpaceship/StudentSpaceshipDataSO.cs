using System;
using System.Collections.Generic;
using API_Mestrado_Lucas;
using Unity.VisualScripting;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    [CreateAssetMenu(fileName = "StudentSpaceshipData", menuName = "ScriptableObjects/SessionsData/StudentSpaceshipData")]
    public class StudentSpaceshipDataSO : ScriptableObject
    {
        public List<StudentWrongAnswersDTO> AllStudentWrongAnswersOfQuiz { get; set; } = new();
        public List<QuizDTO> AllQuizesOfTeacher { get; set; } = new();
        
        public QuizDTO CurrentSelectedQuiz { get; set; }
        public StudentCompleteInfoDTO CurrentSelectedStudent { get; set; }
        public List<SessionDTO> AllSpaceshipSessionsOfTeacher { get; set; } = new();
        public List<SessionDTO> CurrentSpaceshipSessionOfGraph { get; set; } = new(); //just to show a date on mouse over, ffs this graph and chart asset
        
        
        public bool QuestionsGot { get; set; }
        
        public event Action insufficientData;
        public event Action<StudentCompleteInfoDTO> studentSelected;
        public event Action<QuizDTO> quizSelected;
        public event Action allQuestionsGot;
        public event Action studentWrongAnswersGot;
        
        public void InsufficientData()
        {
            insufficientData?.Invoke();
        }

        public void StudentSelected(StudentCompleteInfoDTO studentDto)
        {
            CurrentSelectedStudent = studentDto;
            studentSelected?.Invoke(studentDto);
        }
        
        public void QuizSelected(QuizDTO quizDto)
        {
            CurrentSelectedQuiz = quizDto;
            quizSelected?.Invoke(quizDto);
        }

        public void AllQuestionsGot()
        {
            QuestionsGot = true;
            allQuestionsGot?.Invoke();
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
            CurrentSelectedStudent = null;
            QuestionsGot = false;
            CurrentSpaceshipSessionOfGraph = new();
        }
    }
}
