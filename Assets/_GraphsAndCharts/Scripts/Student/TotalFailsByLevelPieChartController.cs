using System;
using System.Linq;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class TotalFailsByLevelPieChartController : MonoBehaviour
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        [SerializeField] private StudentsSessionsDataSO studentsSessionsData;

        [SerializeField] private PieChart pieChart;
        [SerializeField] private Material pieMaterial;

        [Space] [SerializeField] private GameObject noDataText;

        private void OnEnable()
        {
            studentsSessionsData.StudentSelectedOnDropdown += FillPieChart;
            studentsSessionsData.ClassInsufficientData += InsufficientData;
        }

        private void OnDisable()
        {
            studentsSessionsData.StudentSelectedOnDropdown -= FillPieChart;
            studentsSessionsData.ClassInsufficientData -= InsufficientData;
        }

        private void InsufficientData()
        {
            NoData();
        }

        private void FillPieChart(StudentAndCompleteLevelsSessionsDTO studentSession)
        {
            noDataText.SetActive(false);
            pieChart.DataSource.StartBatch();
            pieChart.DataSource.Clear();

            var completeLevelSessions = studentSession.CompleteLevelSessions.Where(x => x.TotalFails > 0)
                .OrderBy(x => x.TotalFails).ToList();

            if (!completeLevelSessions.Exists(x => x.TotalFails > 0))
            {
                noDataText.SetActive(true);
                pieChart.DataSource.EndBatch();

                return;
            }

            var cont = 0;

            var colorChangeValue = Convert.ToInt32(255 / Mathf.Clamp(completeLevelSessions.Count - 1,1,completeLevelSessions.Count - 1));
            var currentColorValue = 0;

            for (var i = 0; i < completeLevelSessions.Count; i++)
            {
                var newMat = new Material(Shader.Find("UI/Unlit/Transparent"));
                newMat.SetColor(Color1, new Color(1, 1, 1));

                if (completeLevelSessions.Count == 1)
                {
                    currentColorValue = 0;
                    Color newColor = new Color32(255, (byte)currentColorValue, (byte)currentColorValue, 255);
                    newMat.SetColor(Color1, newColor);
                }
                else if (i != 0)
                {
                    currentColorValue -= Mathf.Clamp(colorChangeValue, 0, 255);
                    Color newColor = new Color32(255, (byte)currentColorValue, (byte)currentColorValue, 255);
                    newMat.SetColor(Color1, newColor);
                }

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

                    pieChart.DataSource.AddCategory(levelNameAndNumber, newMat);
                    pieChart.DataSource.SetValue(levelNameAndNumber, completeLevelSessions[i].TotalFails);
                }
                if (completeLevelSessions[i].QuizId != null)
                {
                    var quizName = completeLevelSessions[i].Quiz.Name;
                    var firstCode = "";
                    var secondCode = "";
                    ColorBySubject.Colorize("Quiz", out firstCode, out secondCode);
                    
                    var quizNameOnChart = "<b>" + firstCode + "QUIZ" + "</b>" + secondCode + " - " +
                                          $"{quizName[..(quizName.Length > 12 ? 12 : quizName.Length)]}{(quizName.Length > 12 ? "..." : "")}";
                    
                    pieChart.DataSource.AddCategory(quizNameOnChart, newMat);
                    pieChart.DataSource.SetValue(quizNameOnChart, completeLevelSessions[i].TotalFails);
                }
            }

            pieChart.DataSource.EndBatch();
        }

        private void NoData()
        {
            noDataText.SetActive(true);
            pieChart.DataSource.StartBatch();
            pieChart.DataSource.Clear();
            pieChart.DataSource.EndBatch();
        }
    }
}