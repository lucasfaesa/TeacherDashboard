using System;
using System.Collections;
using System.Linq;
using _TeacherDashboard.Scripts;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GraphsAndCharts.Scripts.StudentSpaceship
{
    public class GraphController : MonoBehaviour
    {
        [SerializeField] private StudentSpaceshipDataSO studentSpaceshipData;
        [Space]
        [SerializeField] private GraphChart graphChart;
        [SerializeField] private HorizontalAxis horizontalAxis;
        [SerializeField] private VerticalAxis verticalAxis;
        [Space] 
        [SerializeField] private GameObject insufficientData;

        private void OnEnable()
        {
            studentSpaceshipData.quizSelected += Generate;
            studentSpaceshipData.studentSelected += Generate;
            studentSpaceshipData.allQuestionsGot += GenerateGraphData;
        }

        private void OnDisable()
        {
            studentSpaceshipData.quizSelected -= Generate;
            studentSpaceshipData.studentSelected -= Generate;
            studentSpaceshipData.allQuestionsGot -= GenerateGraphData;
        }

        private void Generate(QuizDTO _)
        {
            GenerateGraphData();
        }

        private void Generate(StudentCompleteInfoDTO _)
        {
            GenerateGraphData();
        }
        
        private void GenerateGraphData()
        {
            if (studentSpaceshipData.CurrentSelectedStudent == null ||
                studentSpaceshipData.CurrentSelectedQuiz == null) return;
            
            graphChart.DataSource.StartBatch();
            graphChart.DataSource.ClearCategory("Points");

            var filteredListByStudent =
                studentSpaceshipData.AllSpaceshipSessionsOfTeacher.Where(x =>
                x.StudentId == studentSpaceshipData.CurrentSelectedStudent.Id 
                && x.QuizId == studentSpaceshipData.CurrentSelectedQuiz.Id).OrderBy(x=>x.FinishedDate).TakeLast(40).ToList();

            studentSpaceshipData.CurrentSpaceshipSessionOfGraph = filteredListByStudent;
            
            if(filteredListByStudent.Count > 0)
                insufficientData.SetActive(false);
            else
            {
                graphChart.DataSource.EndBatch();
                insufficientData.SetActive(true);
                horizontalAxis.SubDivisions.Total = 0;
                horizontalAxis.MainDivisions.Total = 0;
                return;
            }
            
            DateTime previousDate = new();
            
            graphChart.DataSource.VerticalViewOrigin = 0;
            var maxScore = studentSpaceshipData.AllSpaceshipSessionsOfTeacher.Max(x => x.Score);

            //fazendo um novo cap vertical pro grafico pegando 10% a mais do valor do maior fail, se convetendo pra int o valor for igual ao anterior, adiciona um, se não usa o valor com 10% a mais novo mesmo
            var newChartCap = Convert.ToInt64(maxScore * 0.1f) + maxScore  == maxScore ? maxScore + 1 : Convert.ToInt64(maxScore * 0.1f) + maxScore;
            graphChart.DataSource.VerticalViewSize = newChartCap;
            //verticalAxis.MainDivisions.UnitsPerDivision = Convert.ToInt32(maxScore * 0.2f) == 0 ? 1 : Convert.ToInt32(maxScore * 0.2f);
            
            horizontalAxis.SubDivisions.Total = filteredListByStudent.Count - 1;
            horizontalAxis.MainDivisions.Total = 1;
            
            for (int i = 0; i < filteredListByStudent.Count; i++)
            {
                graphChart.DataSource.AddPointToCategory("Points", i, filteredListByStudent[i].Score);

                if (filteredListByStudent[i].PlayedDate.Date != previousDate.Date){
                    previousDate = filteredListByStudent[i].PlayedDate;
                    graphChart.HorizontalValueToStringMap[i] = filteredListByStudent[i].PlayedDate.ToString("dd/MM/yy");
                }
                else
                    graphChart.HorizontalValueToStringMap[i] = "";
            }
            
            graphChart.DataSource.EndBatch();
        }
    }
}
