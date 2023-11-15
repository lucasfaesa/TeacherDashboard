using System.Linq;
using API_Mestrado_Lucas;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class RightSideInfosController : MonoBehaviour
    {
        [SerializeField] private StudentsSessionsDataSO studentsSessionsData;

        [Space] [SerializeField] private TextMeshProUGUI dateText;

        [SerializeField] private TextMeshProUGUI highestScoreText;

        private void OnEnable()
        {
            studentsSessionsData.StudentSelectedOnDropdown += FillTexts;
            studentsSessionsData.ClassInsufficientData += InsufficientData;
        }

        private void OnDisable()
        {
            studentsSessionsData.StudentSelectedOnDropdown -= FillTexts;
            studentsSessionsData.ClassInsufficientData -= InsufficientData;
        }

        private void InsufficientData()
        {
            NoData();
        }

        private void FillTexts(StudentAndCompleteLevelsSessionsDTO studentSession)
        {
            ResetTexts();

            if (studentsSessionsData.CompleteStudentsSession.Count == 0) return;

            dateText.text = studentSession.CompleteLevelSessions.OrderByDescending(x => x.LastPlayedDate)
                .FirstOrDefault().LastPlayedDate.Value.ToString("dd/MM/yyyy");

            var highScore = studentSession.CompleteLevelSessions
                .OrderByDescending(x => x.MaxScore).FirstOrDefault();

            if (highScore == null) return;
            
            if (highScore.LevelId != null)
            {
                var highScoreLevel =
                    studentsSessionsData.TotalLevelsAndNumbers.FirstOrDefault(
                        x => x.LevelDto.Id == highScore.LevelId);

                highestScoreText.text = highScoreLevel.LevelDto.SubjectTheme.Name + " - Nível " + highScoreLevel.number +
                                        " [" + highScore.MaxScore + (highScore.MaxScore > 1 ? " pontos" : " ponto") + "]";    
            }
            else
            {
                var quizName = highScore.Quiz.Name;
                highestScoreText.text =
                    $"Quiz: {quizName[..(quizName.Length > 25 ? 25 : quizName.Length)]}{(quizName.Length > 25 ? "..." : "")} [{highScore.MaxScore + (highScore.MaxScore > 1 ? " pontos" : " ponto")}]";
            }
            
        }


        private void ResetTexts()
        {
            dateText.text = "";
            highestScoreText.text = "";
        }

        private void NoData()
        {
            dateText.text = "Dados insuficientes";
            highestScoreText.text = "Dados insuficientes";
        }
    }
}