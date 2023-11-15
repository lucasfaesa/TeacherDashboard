using System.Collections.Generic;
using System.Linq;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.General
{
    public class TotalPlaysBarChartController : MonoBehaviour
    {
        [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;

        [SerializeField] private BarChart barChart;
        [SerializeField] private CanvasBarChart canvasBarChart;

        [SerializeField] private Material barMaterial;

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

            dto = dto.OrderBy(x => x.TotalPlays).ToList();

            if (!dto.Exists(x => x.TotalPlays > 0))
            {
                noDataText.SetActive(true);
                barChart.DataSource.EndBatch();

                return;
            }

            if (dto.Count > 27) canvasBarChart.FitToContainer = true;

            for (var i = 0; i < dto.Count; i++)
            {
                barChart.DataSource.AddCategory(dto[i].Student.Name, barMaterial);
                barChart.DataSource.SetValue(dto[i].Student.Name, "All", dto[i].TotalPlays);
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
}