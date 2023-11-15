using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TeacherDashboard.Scripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _GraphsAndCharts.Scripts.GroupClassSpaceship
{
    public class ListViewController : MonoBehaviour
    {
        [SerializeField] private GroupClassSpaceshipDataSO groupClassSpaceshipData;
        [SerializeField] private TeacherDataSO teacherData;
        [Space] 
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
        [SerializeField] private Transform instantiationContent;
        [SerializeField] private ListViewDisplay listViewPrefab;
        [SerializeField] private List<ListViewDisplay> listViewPool;
        [SerializeField] private GameObject noData;

        private void OnEnable()
        {
            groupClassSpaceshipData.allQuizesOfTeacherGot += FillList;
            groupClassSpaceshipData.studentWrongAnswersGot += FillList;
        }

        private void OnDisable()
        {
            groupClassSpaceshipData.allQuizesOfTeacherGot -= FillList;
            groupClassSpaceshipData.studentWrongAnswersGot -= FillList;
        }
        
        private void FillList()
        {
            noData.SetActive(false);
            foreach (var listDisplay in listViewPool) listDisplay.gameObject.SetActive(false);

            if (groupClassSpaceshipData.AllStudentWrongAnswersOfQuiz.Count == 0)
            {
                noData.SetActive(true);
                return;
            }
            
            if (listViewPool.Count < QuestionAndPercentageList().Count)
                InstantiateMoreListObjects(QuestionAndPercentageList().Count - listViewPool.Count);

            var wrongAnswers = QuestionAndPercentageList();

            for (var i = 0; i < wrongAnswers.Count; i++)
            {
                listViewPool[i].SetData(wrongAnswers[i].QuestionTitle, wrongAnswers[i].ErrorPercentageTotal,
                    wrongAnswers[i].ErrorPercentageStudents, wrongAnswers[i].CommonWrongResponse);
                listViewPool[i].gameObject.SetActive(true);
            }

            UpdateCanvas();
        }

        public List<QuestionAndPercentage> QuestionAndPercentageList()
        {
            var quizQuestions =
                groupClassSpaceshipData.AllQuizesOfTeacher.Find(x => x.Id == groupClassSpaceshipData.CurrentSelectedQuiz.Id).Questions.ToList();

            var wrongQuestionsGroupedList = groupClassSpaceshipData.AllStudentWrongAnswersOfQuiz.GroupBy(x => x.QuestionTitle)
                                                            .OrderByDescending(x=>x.Count());

            int totalWrongAnswers = groupClassSpaceshipData.AllStudentWrongAnswersOfQuiz.Count;

            List<QuestionAndPercentage> questAndPercentageList = new();
            
            var studentsListOfThisGroupClass = teacherData.TeacherDto.Students.Where(x => x.GroupClassId ==
                teacherData.CurrentlyChosenGroupClassId).ToList();
            
            foreach (var group in wrongQuestionsGroupedList)
            {
                List<int> studentsThatAnsweredWrongly = new();
                
                foreach (var wrongAnswer in group) //pegando porcentagem de alunos que erraram
                {
                    if (studentsListOfThisGroupClass.Any(x=>x.Id == wrongAnswer.StudentId)
                        && !studentsThatAnsweredWrongly.Contains(wrongAnswer.StudentId))
                    {
                        studentsThatAnsweredWrongly.Add(wrongAnswer.StudentId);
                    }
                }

                var mostCommonWrongResponse = group.GroupBy(x => x.StudentWrongAnswer).OrderByDescending(x => x.Count())
                    .First().Key;
                
                questAndPercentageList.Add(new QuestionAndPercentage(group.Key, Convert.ToInt32(((float)group.Count()/totalWrongAnswers)*100).ToString(), 
                                                                                        Convert.ToInt32(((float)studentsThatAnsweredWrongly.Count/studentsListOfThisGroupClass.Count)*100).ToString(),
                                                                                        mostCommonWrongResponse));    
            }
            
            foreach (var questionAndPercentage in questAndPercentageList)
            {
                quizQuestions = quizQuestions.Where(x => x.QuestionTitle != questionAndPercentage.QuestionTitle).ToList(); //removendo as perguntas que já estão na lista de erros
            }

            if (quizQuestions.Count > 0)
            {
                foreach (var question in quizQuestions)
                {
                    questAndPercentageList.Add(new QuestionAndPercentage(question.QuestionTitle, "0", "0", "-"));
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
            
            //Canvas.ForceUpdateCanvases();
            verticalLayoutGroup.enabled = true;
            Canvas.ForceUpdateCanvases();
        }
    }

    public class QuestionAndPercentage
    {
        public string QuestionTitle { get; set; }
        public string ErrorPercentageTotal { get; set; }
        public string ErrorPercentageStudents { get; set; }
        public string CommonWrongResponse { get; set; }

        public QuestionAndPercentage(string title, string percentageTotal, string percentageStudents, string commonResp)
        {
            QuestionTitle = title;
            ErrorPercentageTotal = percentageTotal;
            ErrorPercentageStudents = percentageStudents;
            CommonWrongResponse = commonResp;
        }
    }
}
