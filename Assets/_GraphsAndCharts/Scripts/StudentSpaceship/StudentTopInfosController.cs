using System.Linq;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class StudentTopInfosController : MonoBehaviour
    {
        [SerializeField] private StudentSpaceshipDataSO studentSpaceshipData;

        [SerializeField] private TextMeshProUGUI mostErrorsText;
        [SerializeField] private TextMeshProUGUI leastErrorsText;

        private void OnEnable()
        {
            mostErrorsText.text = "";
            leastErrorsText.text = "";

            studentSpaceshipData.allQuestionsGot += Fill;
            studentSpaceshipData.studentSelected += FillTexts;
        }

        private void OnDisable()
        {
            studentSpaceshipData.allQuestionsGot -= Fill;
            studentSpaceshipData.studentSelected -= FillTexts;
        }

        private void Fill()
        {
            FillTexts(studentSpaceshipData.CurrentSelectedStudent);
        }
        private void FillTexts(StudentCompleteInfoDTO studentDto)
        {
            if (studentSpaceshipData.AllStudentWrongAnswersOfQuiz.Count > 0)
            {
                var studentWrongAnswers = studentSpaceshipData.AllStudentWrongAnswersOfQuiz
                    .Where(x => x.StudentId == studentDto.Id).ToList();

                if (studentWrongAnswers.Count > 0)
                {
                    var mostErrorsQuestion = studentWrongAnswers.GroupBy(x => x.QuestionTitle)
                                                                        .OrderByDescending(x => x.Count());
                    
                    mostErrorsText.text = mostErrorsQuestion.First().Key;
                    leastErrorsText.text = mostErrorsQuestion.Last().Key;    
                }
                else
                {
                    mostErrorsText.text = "Sem dados suficientes";
                    leastErrorsText.text = "Sem dados suficientes";
                }
                
            }
            else
            {
                mostErrorsText.text = "Sem dados suficientes";
                leastErrorsText.text = "Sem dados suficientes";
            }
        }
    }
}

