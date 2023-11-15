using System.Linq;
using _GraphsAndCharts.Scripts.General;
using API_Mestrado_Lucas;
using ChartAndGraph;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class LeftSideInfosAndLevelPieController : MonoBehaviour
    {
        [SerializeField] private StudentsSessionsDataSO studentsSessionsData;
        [SerializeField] private LevelsDataSO levelsDataSo;    
        
        [Header("Chart")] [SerializeField] private PieChart completedLevelsPieChart;

        [SerializeField] private TextMeshProUGUI completedLevelsText;
        [SerializeField] private Material blueMat;
        [SerializeField] private Material blackMat;

        [Header("Texts")] [SerializeField] private TextMeshProUGUI mostFailsText;

        [SerializeField] private TextMeshProUGUI lessFailsText;
        [SerializeField] private TextMeshProUGUI playsTheMostText;
        [SerializeField] private TextMeshProUGUI playsLessText;

        private void Awake()
        {
            ResetTexts();
        }

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

            if (studentsSessionsData.CompleteStudentsSession.Count == 0)
            {
                NoData();
                return;
            }

            var failsList = studentSession.CompleteLevelSessions.Where(x => x.TotalFails > 0)
                .OrderByDescending(x => x.TotalFails).ToList();
            var playTimeList = studentSession.CompleteLevelSessions.Where(x => x.TotalElapsedTime > 0)
                .OrderByDescending(x => x.TotalElapsedTime).ToList();

            var mostFails = failsList.FirstOrDefault();
            var lessFails = failsList.LastOrDefault();
            var playsMost = playTimeList.FirstOrDefault();
            var playsLess = playTimeList.LastOrDefault();
            
            var completedLevelsQuantity =
                studentSession.CompleteLevelSessions.Where(x => x.Finished == true).ToList().Count;

            if (mostFails != null)
            {
                if (mostFails.LevelId != null)
                {
                    var mostFailedLevel =
                        studentsSessionsData.TotalLevelsAndNumbers.FirstOrDefault(x =>
                            x.LevelDto.Id == mostFails.LevelId);
                    mostFailsText.text = mostFailedLevel.LevelDto.SubjectTheme.Name + " - Nível " + mostFailedLevel.number +
                                         " [" + mostFails.TotalFails + " Falhas]";    
                }
                else
                {
                    var quizName = mostFails.Quiz.Name;
                    mostFailsText.text =
                        $"Quiz: {quizName[..(quizName.Length > 25 ? 25 : quizName.Length)]}{(quizName.Length > 25 ? "..." : "")} [{mostFails.TotalFails} Falhas]";
                }
                
            }
            else
            {
                mostFailsText.text = "Dados insuficientes";
            }

            if (lessFails != null)
            {
                if (lessFails.LevelId != null)
                {
                    var lessFailedLevel =
                        studentsSessionsData.TotalLevelsAndNumbers.FirstOrDefault(x =>
                            x.LevelDto.Id == lessFails.LevelId);
                    lessFailsText.text = lessFailedLevel.LevelDto.SubjectTheme.Name + " - Nível " + lessFailedLevel.number +
                                         " [" + lessFails.TotalFails + " Falhas]";
                }
                else
                {
                    var quizName = lessFails.Quiz.Name;
                    lessFailsText.text =
                        $"Quiz: {quizName[..(quizName.Length > 25 ? 25 : quizName.Length)]}{(quizName.Length > 25 ? "..." : "")} [{lessFails.TotalFails} Falhas]";
                }
                
            }
            else
            {
                lessFailsText.text = "Dados insuficientes";
            }

            if (playsMost != null)
            {
                if (playsMost.LevelId != null)
                {
                    var mostPlay =
                        studentsSessionsData.TotalLevelsAndNumbers.FirstOrDefault(x =>
                            x.LevelDto.Id == playsMost.LevelId);
                    playsTheMostText.text = mostPlay.LevelDto.SubjectTheme.Name + " - Nível " + mostPlay.number + " [" +
                                            Mathf.CeilToInt(playsMost.TotalElapsedTime / 60) +
                                            (Mathf.CeilToInt(playsMost.TotalElapsedTime / 60) > 1 ? " minutos]" : " minuto]");
                }
                else
                {
                    var quizName = playsMost.Quiz.Name;
                    playsTheMostText.text = $"Quiz: {quizName[..(quizName.Length > 25 ? 25 : quizName.Length)]}{(quizName.Length > 25 ? "..." : "")} " +
                                            $"[{Mathf.CeilToInt(playsMost.TotalElapsedTime / 60)} {(Mathf.CeilToInt(playsMost.TotalElapsedTime / 60) > 1 ? "minutos" : "minuto")}]";
                }
                
            }
            else
            {
                playsTheMostText.text = "Dados insuficientes";
            }

            if (playsLess != null)
            {
                if (playsLess.LevelId != null)
                {
                    var lessPlay =
                        studentsSessionsData.TotalLevelsAndNumbers.FirstOrDefault(x =>
                            x.LevelDto.Id == playsLess.LevelId);
                    playsLessText.text = lessPlay.LevelDto.SubjectTheme.Name + " - Nível " + lessPlay.number + " [" +
                                         Mathf.CeilToInt(playsLess.TotalElapsedTime / 60) +
                                         (Mathf.CeilToInt(playsLess.TotalElapsedTime / 60) > 1 ? " minutos]" : " minuto]");
                }
                else
                {
                    var quizName = playsLess.Quiz.Name;
                    playsLessText.text = $"Quiz: {quizName[..(quizName.Length > 25 ? 25 : quizName.Length)]}{(quizName.Length > 25 ? "..." : "")} " +
                                            $"[{Mathf.CeilToInt(playsLess.TotalElapsedTime / 60)} {(Mathf.CeilToInt(playsLess.TotalElapsedTime / 60) > 1 ? "minutos" : "minuto")}]";
                }
                
            }
            else
            {
                playsLessText.text = "Dados insuficientes";
            }

            #region PieChart

            var totalLevelsOfGroupClass = studentsSessionsData.LevelsAndNumbersOfGroupClass.Count + levelsDataSo.ActiveQuizesOfThisTeacher.Count;
            
            completedLevelsText.text = completedLevelsQuantity + " / " + totalLevelsOfGroupClass;

            
            FillPieChart(completedLevelsQuantity, totalLevelsOfGroupClass - completedLevelsQuantity);

            #endregion
        }

        private void FillPieChart(float currentValue, float maxValue)
        {
            completedLevelsPieChart.DataSource.StartBatch();
            completedLevelsPieChart.DataSource.Clear();

            completedLevelsPieChart.DataSource.AddCategory("NotCompleted", blackMat);
            completedLevelsPieChart.DataSource.SetValue("NotCompleted", maxValue);

            completedLevelsPieChart.DataSource.AddCategory("Completed", blueMat);
            completedLevelsPieChart.DataSource.SetValue("Completed", currentValue);

            completedLevelsPieChart.DataSource.EndBatch();
        }

        private void ResetTexts()
        {
            mostFailsText.text = "";
            lessFailsText.text = "";
            playsTheMostText.text = "";
            playsLessText.text = "";
        }

        private void NoData()
        {
            mostFailsText.text = "Dados insuficientes";
            lessFailsText.text = "Dados insuficientes";
            playsTheMostText.text = "Dados insuficientes";
            playsLessText.text = "Dados insuficientes";

            completedLevelsPieChart.DataSource.StartBatch();
            completedLevelsPieChart.DataSource.Clear();
            completedLevelsPieChart.DataSource.EndBatch();

            completedLevelsText.text = "Dados insuficientes";
        }
    }
}