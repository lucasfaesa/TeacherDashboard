using System.Linq;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class TriesByLevelBarChartController : MonoBehaviour
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

            var completeLevelSessions = studentSession.CompleteLevelSessions.OrderBy(x => x.TotalTries).ToList();

            if (!completeLevelSessions.Exists(x => x.TotalTries > 0))
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
                    barChart.DataSource.SetValue(levelNameAndNumber, "All", completeLevelSessions[i].TotalTries);
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
                    barChart.DataSource.SetValue(quizNameOnChart, "All", completeLevelSessions[i].TotalTries);
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