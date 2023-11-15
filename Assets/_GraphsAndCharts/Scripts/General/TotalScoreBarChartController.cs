using System.Collections.Generic;
using System.Linq;
using _GraphsAndCharts.Scripts.General;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts
{
    public class TotalScoreBarChartController : MonoBehaviour
    {
        [SerializeField] private HoverChartsDataSO hoverChartsData;
        [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;

        [Space] [SerializeField] private BarChart barChart;

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

        public void OnItemClick(BarChart.BarEventArgs args)
        {
            Debug.Log("point clicked: " + args.Category + " " + args.Value + " " + args.Group);
        }

        private void FillBarChart(List<CompleteStudentSessionDTO> dto)
        {
            noDataText.SetActive(false);
            barChart.DataSource.StartBatch();
            barChart.DataSource.ClearCategories();

            dto = dto.OrderBy(x => x.TotalScore).ToList();

            if (!dto.Exists(x => x.TotalScore > 0))
            {
                noDataText.SetActive(true);
                barChart.DataSource.EndBatch();

                return;
            }

            var cont = 0;

            for (var i = 0; i < dto.Count; i++)
            {
                barChart.DataSource.AddCategory(dto[i].Student.Name, barMaterial);
                barChart.DataSource.SetValue(dto[i].Student.Name, "All", dto[i].TotalScore);
            }

            /*
            var cont = 0;
            for (int j = 0; j < 15; j++)
            {
                for (int i = 0; i < dto.Count; i++)
                {
                    barChart.DataSource.AddCategory(dto[i].Student.Name + cont.ToString(),barMaterial);
                    barChart.DataSource.SetValue(dto[i].Student.Name + cont.ToString(), "All", dto[i].TotalScore);
                    cont++;
                }
            }
            */


            barChart.DataSource.EndBatch();
        }
    }
}