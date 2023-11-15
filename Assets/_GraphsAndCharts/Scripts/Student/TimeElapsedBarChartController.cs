using System.Linq;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.Student
{
    public class TimeElapsedBarChartController : MonoBehaviour
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

            var completeLevelSessions = studentSession.CompleteLevelSessions.OrderBy(x => x.TotalElapsedTime).ToList();

            if (!completeLevelSessions.Exists(x => x.TotalElapsedTime > 0))
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

                    var levelNameAndNumber = "<b>" + firstCode + subjectName[..3].ToUpper() + "</b>" + secondCode + " - " +
                                             subjectThemeName.Split()[0] + " [" + currentLevel.number + "]";

                    barChart.DataSource.AddCategory(levelNameAndNumber, barMaterial);
                    barChart.DataSource.SetValue(levelNameAndNumber, "All", completeLevelSessions[i].TotalElapsedTime > 60 ? completeLevelSessions[i].TotalElapsedTime / 60 : 1);
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
                    barChart.DataSource.SetValue(quizNameOnChart, "All", completeLevelSessions[i].TotalElapsedTime > 60 ? completeLevelSessions[i].TotalElapsedTime / 60 : 1);
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

        /*
        [Header("SO")]
        [SerializeField] private StudentsSessionsDataSO studentsSessionsData;
        [Space]
        [SerializeField] private GraphChart graphChart;
        [SerializeField] private HorizontalAxis horizontalAxis;
        [SerializeField] private VerticalAxis verticalAxis;

        [Header("Debug")]
        [SerializeField] private int studentsQuantity = 50;
        
        // Start is called before the first frame update
        private IEnumerator Start()
        {
            while (!studentsSessionsData.GotStudentsSessions) yield return null;
            while (studentsSessionsData.CurrentSelectedStudentSession == null) yield return null;
            
            FillGraphChart(studentsSessionsData.CurrentSelectedStudentSession);
        }

        private void FillGraphChart(StudentAndCompleteLevelsSessionsDTO studentSession)
        {
            graphChart.DataSource.StartBatch();

            var completeLevelSessions = studentSession.CompleteLevelSessions.OrderBy(x => x.TotalElapsedTime).ToList();

            graphChart.DataSource.VerticalViewOrigin = 0;

            var highestElapsedTime = completeLevelSessions[completeLevelSessions.Count - 1].TotalElapsedTime/60;
            
            //fazendo um novo cap vertical pro grafico pegando 10% a mais do valor do maior fail, se convetendo pra int o valor for igual ao anterior, adiciona um, se não usa o valor com 10% a mais novo mesmo
            var newChartCap = Convert.ToInt64(highestElapsedTime * 0.1f) + highestElapsedTime  == highestElapsedTime ? highestElapsedTime + 1 : Convert.ToInt64(highestElapsedTime * 0.1f) + highestElapsedTime;
            
            verticalAxis.MainDivisions.UnitsPerDivision = Convert.ToInt32(highestElapsedTime * 0.2f) == 0 ? 1 : Convert.ToInt32(highestElapsedTime * 0.2f);
            
            //#DEBUG
            //var newChartCap = studentsQuantity + 5;
            //verticalAxis.MainDivisions.UnitsPerDivision = studentsQuantity * 0.2f;
            
            graphChart.DataSource.VerticalViewSize = newChartCap;
            graphChart.DataSource.ClearCategory("ElapsedTime");
            
            
            for (int i = 0; i < completeLevelSessions.Count; i++)
            {
                var currentLevel =
                    studentsSessionsData.LevelsAndNumbersOfGroupClass.FirstOrDefault(z =>
                        z.LevelDto.Id == completeLevelSessions[i].LevelId);
                
                var levelNameAndNumber = currentLevel.LevelDto.SubjectTheme.Name + " Nivel: " + currentLevel.number;
                
                graphChart.DataSource.AddPointToCategory("ElapsedTime",i,completeLevelSessions[i].TotalElapsedTime/60);
                graphChart.HorizontalValueToStringMap[i] = levelNameAndNumber;
            }
            
            //### DEBUG
            /*var cont = 0;
            //canvasBarChart.FitToContainer = true;
            for (int j = 0; j < studentsQuantity; j++)
            {
               graphChart.DataSource.AddPointToCategory("StudentsFaults",j, cont++);
            }*/
        //horizontalAxis.SubDivisions.Total = studentsQuantity;
        /*
            horizontalAxis.SubDivisions.Total = completeLevelSessions.Count - 1;
            horizontalAxis.MainDivisions.Total = 1;
            
            graphChart.DataSource.EndBatch();
        }*/
    }
}