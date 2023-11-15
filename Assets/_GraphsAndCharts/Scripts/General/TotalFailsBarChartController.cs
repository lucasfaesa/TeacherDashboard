using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.General
{
    public class TotalFailsBarChartController : MonoBehaviour
    {
        /*[Header("SO")]
        [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;
        [Space]
        [SerializeField] private GraphChart graphChart;
        [SerializeField] private HorizontalAxis horizontalAxis;
        [SerializeField] private VerticalAxis verticalAxis;

        [Header("Debug")]
        [SerializeField] private int studentsQuantity = 50;*/

        [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;

        [Space] [SerializeField] private BarChart barChart;

        [SerializeField] private CanvasBarChart canvasBarChart;

        [Space] [SerializeField] private Material barMaterial;

        [Space] [SerializeField] private GameObject noDataText;

        private void OnEnable()
        {
            groupClassSessionsData.OnNewSessionsGot += FillChart;
        }

        private void OnDisable()
        {
            groupClassSessionsData.OnNewSessionsGot -= FillChart;
        }

        private void FillChart()
        {
            FillBarChart(groupClassSessionsData.CompleteGroupClassSession);
        }

        private void FillBarChart(List<CompleteStudentSessionDTO> dto)
        {
            noDataText.SetActive(false);
            barChart.DataSource.StartBatch();
            barChart.DataSource.ClearCategories();

            dto = dto.OrderBy(x => x.TotalFails).ToList();

            if (!dto.Exists(x => x.TotalFails > 0))
            {
                noDataText.SetActive(true);
                barChart.DataSource.EndBatch();

                return;
            }

            if (dto.Count > 27) canvasBarChart.FitToContainer = true;

            for (var i = 0; i < dto.Count; i++)
            {
                barChart.DataSource.AddCategory(dto[i].Student.Name, barMaterial);
                barChart.DataSource.SetValue(dto[i].Student.Name, "All", dto[i].TotalFails);
            }


            /*var cont = 0;
            //canvasBarChart.FitToContainer = true;
            for (int j = 0; j < 28; j++)
            {
                for (int i = 0; i < dto.Count; i++)
                {
                    barChart.DataSource.AddCategory(dto[i].Student.Name + cont.ToString(),barMaterial);
                    barChart.DataSource.SetValue(dto[i].Student.Name + cont.ToString(), "All", dto[i].TotalPlays);
                    cont++;
                }
            }*/


            barChart.DataSource.EndBatch();
        }
    }
    /*
        // Start is called before the first frame update
        private IEnumerator Start()
        {
            while (!groupClassSessionsData.GotSessions) yield return null;
            
            FillGraphChart(groupClassSessionsData.CompleteGroupClassSession);
        }

        private void FillGraphChart(List<CompleteStudentSessionDTO> completeSession)
        {
            graphChart.DataSource.StartBatch();

            completeSession = completeSession.OrderBy(x => x.TotalFails).ToList();
            
            graphChart.DataSource.VerticalViewOrigin = 0;

            var highestTotalFail = completeSession[completeSession.Count - 1].TotalFails;
            
            //fazendo um novo cap vertical pro grafico pegando 10% a mais do valor do maior fail, se convetendo pra int o valor for igual ao anterior, adiciona um, se não usa o valor com 10% a mais novo mesmo
            var newChartCap = Convert.ToInt64(highestTotalFail * 0.1f) + highestTotalFail  == highestTotalFail ? highestTotalFail + 1 : Convert.ToInt64(highestTotalFail * 0.1f) + highestTotalFail;
            
            verticalAxis.MainDivisions.UnitsPerDivision = Convert.ToInt32(highestTotalFail * 0.2f) == 0 ? 1 : Convert.ToInt32(highestTotalFail * 0.2f);
            
            //#DEBUG
            //var newChartCap = studentsQuantity + 5;
            //verticalAxis.MainDivisions.UnitsPerDivision = studentsQuantity * 0.2f;
            
            graphChart.DataSource.VerticalViewSize = newChartCap;
            graphChart.DataSource.ClearCategory("StudentsFaults");
            
            
            for (int i = 0; i < completeSession.Count; i++)
            {
                graphChart.DataSource.AddPointToCategory("StudentsFaults",i,completeSession[i].TotalFails);
                graphChart.HorizontalValueToStringMap[i] = completeSession[i].Student.Name;
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
            horizontalAxis.SubDivisions.Total = completeSession.Count - 1;
            horizontalAxis.MainDivisions.Total = 1;
            
            graphChart.DataSource.EndBatch();
        }
    }
    */
}