using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using UnityEngine;
using UnityEngine.UI;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class StudentSpaceshipListViewController : MonoBehaviour
    {
        [SerializeField] private StudentSpaceshipDataSO studentSpaceshipData;
        [Space] 
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
        [SerializeField] private Transform instantiationContent;
        [SerializeField] private StudentSpaceshipListViewDisplay listViewPrefab;
        [SerializeField] private List<StudentSpaceshipListViewDisplay> listViewPool;
        [SerializeField] private GameObject noData;

        private void OnEnable()
        {
            studentSpaceshipData.studentSelected += FillList;
            studentSpaceshipData.allQuestionsGot += Fill;
            studentSpaceshipData.studentWrongAnswersGot += Fill;
        }

        private void OnDisable()
        {
            studentSpaceshipData.studentSelected -= FillList;
            studentSpaceshipData.allQuestionsGot -= Fill;
            studentSpaceshipData.studentWrongAnswersGot -= Fill;
        }

        private void Fill()
        {
            FillList(studentSpaceshipData.CurrentSelectedStudent);
        }
        
        private void FillList(StudentCompleteInfoDTO studentCompleteInfoDto)
        {
            if (studentSpaceshipData.CurrentSelectedQuiz == null ||
                studentSpaceshipData.CurrentSelectedStudent == null)
                return;
            
            noData.SetActive(false);
            foreach (var listDisplay in listViewPool) listDisplay.gameObject.SetActive(false);

            var questionsAndPercentage = QuestionAndPercentageList();
            
            if (studentSpaceshipData.AllStudentWrongAnswersOfQuiz.Count == 0 || questionsAndPercentage == null)
            {
                noData.SetActive(true);
                return;
            }

            if (listViewPool.Count < questionsAndPercentage.Count)
                InstantiateMoreListObjects(QuestionAndPercentageList().Count - listViewPool.Count);

            for (var i = 0; i < questionsAndPercentage.Count; i++)
            {
                listViewPool[i].SetData(questionsAndPercentage[i].QuestionTitle, questionsAndPercentage[i].ErrorPercentageTotal, questionsAndPercentage[i].CommonWrongResponse);
                listViewPool[i].gameObject.SetActive(true);
            }

            UpdateCanvas();
        }

        public List<StudentQuestionAndPercentage> QuestionAndPercentageList()
        {
            var quizQuestions =
                studentSpaceshipData.AllQuizesOfTeacher.Find(x => x.Id == studentSpaceshipData.CurrentSelectedQuiz.Id).Questions.ToList();

            var wrongQuestionsOfStudentGroupedList = studentSpaceshipData.AllStudentWrongAnswersOfQuiz
                                                            .Where(x=>x.StudentId == studentSpaceshipData.CurrentSelectedStudent.Id)
                                                                .GroupBy(x => x.QuestionTitle)
                                                                    .OrderByDescending(x=>x.Count());

            var sessionsOfThisStudent = studentSpaceshipData.AllSpaceshipSessionsOfTeacher.Where(x =>
                x.StudentId == studentSpaceshipData.CurrentSelectedStudent.Id
                && x.QuizId == studentSpaceshipData.CurrentSelectedQuiz.Id).ToList();

            if (sessionsOfThisStudent.Count == 0) //se o aluno não tiver nenhuma sessão é pq ele não jogou nenhuma vez
                return null;
            
            int totalWrongAnswers = studentSpaceshipData.AllStudentWrongAnswersOfQuiz.Count(x => x.StudentId == studentSpaceshipData.CurrentSelectedStudent.Id);

            List<StudentQuestionAndPercentage> questAndPercentageList = new();

            foreach (var group in wrongQuestionsOfStudentGroupedList)
            {
                var mostCommonWrongResponse = group.GroupBy(x => x.StudentWrongAnswer).OrderByDescending(x => x.Count())
                    .First().Key;
                
                questAndPercentageList.Add(new StudentQuestionAndPercentage(group.Key, Convert.ToInt32(((float)group.Count()/totalWrongAnswers)*100).ToString(), mostCommonWrongResponse));    
            }
            
            foreach (var questionAndPercentage in questAndPercentageList)
            {
                quizQuestions = quizQuestions.Where(x => x.QuestionTitle != questionAndPercentage.QuestionTitle).ToList(); //removendo as perguntas que já estão na lista de erros
            }

            if (quizQuestions.Count > 0)
            {
                foreach (var question in quizQuestions)
                {
                    questAndPercentageList.Add(new StudentQuestionAndPercentage(question.QuestionTitle, "0", "-"));
                }
            }
            
            
            return questAndPercentageList;
        }
        
        private void InstantiateMoreListObjects(int quantity)
        {
            for (var i = 0; i < quantity; i++)
            {
                var newObject = Instantiate(listViewPrefab, instantiationContent);

                listViewPool.Add(newObject);
            }
        }

        private void UpdateCanvas()
        {
            StartCoroutine(UpdateCanvasRoutine());
        }
        
        private IEnumerator UpdateCanvasRoutine()
        {
            //Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            verticalLayoutGroup.enabled = false;
            
            Canvas.ForceUpdateCanvases();
            verticalLayoutGroup.enabled = true;
            Canvas.ForceUpdateCanvases();
        }
    }

    public class StudentQuestionAndPercentage
    {
        public string QuestionTitle { get; set; }
        public string ErrorPercentageTotal { get; set; }
        public string CommonWrongResponse { get; set; }

        public StudentQuestionAndPercentage(string title, string percentageTotal, string commonResp)
        {
            QuestionTitle = title;
            ErrorPercentageTotal = percentageTotal;
            CommonWrongResponse = commonResp;
        }
    }
}

