using System;
using System.Collections.Generic;
using System.Linq;
using _GraphsAndCharts.Scripts.General;
using API_Mestrado_Lucas;
using ChartAndGraph;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GraphsAndCharts.Scripts
{
    public class MaxScorePieChartController : MonoBehaviour
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;

        [SerializeField] private PieChart pieChart;
        [SerializeField] private Material pieMaterial;

        [Space] [SerializeField] private GameObject noDataText;

        [Header("Debug")] [SerializeField] private bool useDebug;

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
            FillPieChart(groupClassSessionsData.CompleteGroupClassSession);
        }

        private void FillPieChart(List<CompleteStudentSessionDTO> dto)
        {
            noDataText.SetActive(false);
            pieChart.DataSource.StartBatch();
            pieChart.DataSource.Clear();

            dto = dto.OrderBy(x => x.MaxScore).ToList();

            if (!dto.Exists(x => x.MaxScore > 0))
            {
                noDataText.SetActive(true);
                pieChart.DataSource.EndBatch();

                return;
            }

            var cont = 0;

            if (!useDebug)
            {
                /*var colorChangeValue = Convert.ToInt32(255 / Mathf.Clamp(dto.Count - 1, 0, dto.Count - 1));
                var currentColorValue = 0;

                for (var i = 0; i < dto.Count; i++)
                {
                    var newMat = new Material(Shader.Find("UI/Unlit/Transparent"));
                    //newMat.SetColor(Color1, new Color(1, 1, 1));
                    
                    if (dto.Count == 1)
                    {
                        currentColorValue = 0;
                        Color newColor = new Color32((byte)currentColorValue, (byte)currentColorValue, 255, 255);
                        newMat.SetColor(Color1, newColor);
                    }
                    else if (i != 0)
                    {
                        currentColorValue -= Mathf.Clamp(colorChangeValue, 0, 255);
                        Color newColor = new Color32((byte)currentColorValue, (byte)currentColorValue, 255, 255);
                        newMat.SetColor(Color1, newColor);
                    }

                    pieChart.DataSource.AddCategory(dto[i].Student.Name, newMat);
                    pieChart.DataSource.SetValue(dto[i].Student.Name, dto[i].MaxScore);
                }*/

                for (var i = 0; i < dto.Count; i++)
                {
                    var newMat = new Material(Shader.Find("UI/Unlit/Transparent"));
                    
                    Color newColor = new Color32((byte)Random.Range(50f, 255f), (byte)Random.Range(50f, 255f), (byte)Random.Range(50f, 255f), 255);
                    newMat.SetColor(Color1, newColor);

                    pieChart.DataSource.AddCategory(dto[i].Student.Name, newMat);
                    pieChart.DataSource.SetValue(dto[i].Student.Name, dto[i].MaxScore);
                }
            }
            else
            {
                var colorChangeValue = Convert.ToInt32(255 / ((dto.Count - 1) * 28));
                var currentColorValue = 0;

                for (var j = 0; j < 28; j++)
                for (var i = 0; i < dto.Count; i++)
                {
                    var newMat = new Material(Shader.Find("UI/Unlit/Transparent"));
                    //newMat.SetColor(Color1, new Color(1, 1, 1));
                    if (i != 0)
                    {
                        currentColorValue -= Mathf.Clamp(colorChangeValue, 0, 255);
                        Color newColor = new Color32((byte)currentColorValue, (byte)currentColorValue, 255, 255);
                        newMat.SetColor(Color1, newColor);
                    }

                    pieChart.DataSource.AddCategory(dto[i].Student.Name + cont, newMat);
                    pieChart.DataSource.SetValue(dto[i].Student.Name + cont, dto[i].MaxScore);

                    cont++;
                }
            }

            pieChart.DataSource.EndBatch();
        }
    }
}