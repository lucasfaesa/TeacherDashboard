using System.Collections.Generic;
using System.Linq;
using _GraphsAndCharts.Scripts.General;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;

public class FinishedLevelBarChartController : MonoBehaviour
{
    [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;

    [SerializeField] private BarChart barChart;
    [SerializeField] private Material barMaterial;

    [Space] [SerializeField] private GameObject noDataText;

    // Start is called before the first frame update
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

        dto = dto.OrderBy(x => x.LevelsFinishedQuantity).ToList();

        if (!dto.Exists(x => x.LevelsFinishedQuantity > 0))
        {
            noDataText.SetActive(true);
            barChart.DataSource.EndBatch();

            return;
        }

        var cont = 0;

        for (var i = 0; i < dto.Count; i++)
        {
            barChart.DataSource.AddCategory(dto[i].Student.Name, barMaterial);
            barChart.DataSource.SetValue(dto[i].Student.Name, "All", dto[i].LevelsFinishedQuantity);
        }


        /*var cont = 0;
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