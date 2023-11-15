using System.Linq;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class TotalPointsByLevelBarChartController : MonoBehaviour
    {
        [Header("SO")] [SerializeField] private StudentsSessionsDataSO studentsSessionsData;

        [Space] [SerializeField] private BarChart barChart;

        [SerializeField] private CanvasBarChart canvasBarChart;
        [SerializeField] private Material barMaterial;

        [Space] [SerializeField] private GameObject noDataText;

        private void OnEnable()
        {
            studentsSessionsData.StudentSelectedOnDropdown += FillBarChart;
            studentsSessionsData.ClassInsufficientData += InsufficientData;
        }

        private void OnDisable()
        {
            studentsSessionsData.StudentSelectedOnDropdown -= FillBarChart;
            studentsSessionsData.ClassInsufficientData -= InsufficientData;
        }

        private void InsufficientData()
        {
            NoData();
        }

        private void FillBarChart(StudentAndCompleteLevelsSessionsDTO studentSession)
        {
            noDataText.SetActive(false);
            barChart.DataSource.StartBatch();
            barChart.DataSource.ClearCategories();

            var completeLevelSessions = studentSession.CompleteLevelSessions.OrderBy(x => x.TotalScore).ToList();

            if (!completeLevelSessions.Exists(x => x.TotalScore > 0))
            {
                noDataText.SetActive(true);
                barChart.DataSource.EndBatch();

                return;
            }

            if (completeLevelSessions.Count > 27) canvasBarChart.FitToContainer = true;

            for (var i = 0; i < completeLevelSessions.Count; i++)
            {
                if (completeLevelSessions[i].LevelId != null)
                {
                    var currentLevel =
                        studentsSessionsData.TotalLevelsAndNumbers.FirstOrDefault(z =>
                            z.LevelDto.Id == completeLevelSessions[i].LevelId);

                    var subjectName = currentLevel.LevelDto.SubjectTheme.Subject.Name;
                    var subjectThemeName = currentLevel.LevelDto.SubjectTheme.Name;
                    var firstCode = "";
                    var secondCode = "";
                    ColorBySubject.Colorize(subjectName, out firstCode, out secondCode);

                    var levelNameAndNumber = "<b>" + firstCode + subjectName[..3].ToUpper() + "</b>" + secondCode +
                                             " - " +
                                             subjectThemeName.Split()[0] + " [" + currentLevel.number + "]";

                    barChart.DataSource.AddCategory(levelNameAndNumber, barMaterial);
                    barChart.DataSource.SetValue(levelNameAndNumber, "All", completeLevelSessions[i].TotalScore);
                }

                if (completeLevelSessions[i].QuizId != null)
                {
                    var quizName = completeLevelSessions[i].Quiz.Name;
                    var firstCode = "";
                    var secondCode = "";
                    ColorBySubject.Colorize("Quiz", out firstCode, out secondCode);
                    
                    var quizNameOnChart = "<b>" + firstCode + "QUIZ" + "</b>" + secondCode + " - " +
                                          $"{quizName[..(quizName.Length > 12 ? 12 : quizName.Length)]}{(quizName.Length > 12 ? "..." : "")}";
                    
                    barChart.DataSource.AddCategory(quizNameOnChart, barMaterial);
                    barChart.DataSource.SetValue(quizNameOnChart, "All", completeLevelSessions[i].TotalScore);
                }
            }

            barChart.DataSource.EndBatch();
        }

        private void NoData()
        {
            noDataText.SetActive(true);
            barChart.DataSource.StartBatch();
            barChart.DataSource.ClearCategories();
            barChart.DataSource.EndBatch();
        }
    }
}

public static class ColorBySubject
{
    public static void Colorize(string subject, out string firstCode, out string secondCode)
    {
        switch (subject)
        {
            case "Matemática":
                firstCode = "<color=red>";
                break;
            case "Geografia":
                firstCode = "<color=green>";
                break;
            case "Ciências":
                firstCode = "<color=#48D1CC>";
                break;
            case "Quiz":
                firstCode = "<color=#FF3BEA>";
                break;
                
            default:
                firstCode = "<color=white>";
                break;
        }

        secondCode = "</color>";
    }
}