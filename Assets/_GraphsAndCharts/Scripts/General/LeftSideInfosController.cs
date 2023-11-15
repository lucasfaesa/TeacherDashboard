using System.Linq;
using ChartAndGraph;
using TMPro;
using UnityEngine;

namespace _GraphsAndCharts.Scripts.General
{
    public class LeftSideInfosController : MonoBehaviour
    {
        [SerializeField] private GroupClassSessionsDataSO groupClassSessionsData;
        [SerializeField] private LevelsDataSO levelsData;

        [Header("Chart")] [SerializeField] private PieChart completedLevelsPieChart;

        [SerializeField] private TextMeshProUGUI completedLevelsText;
        [SerializeField] private Material blueMat;
        [SerializeField] private Material blackMat;

        [Header("Texts")] [SerializeField] private TextMeshProUGUI mostFailsText;

        [SerializeField] private TextMeshProUGUI lessFailsText;
        [SerializeField] private TextMeshProUGUI playsTheMost;
        [SerializeField] private TextMeshProUGUI mostCompletedLevels;
        [SerializeField] private TextMeshProUGUI lessLevelsCompleted;

        private void OnEnable()
        {
            groupClassSessionsData.OnSessionsAndLevelsGot += Fill;
        }

        private void OnDisable()
        {
            groupClassSessionsData.OnSessionsAndLevelsGot -= Fill;
        }

        private void Fill()
        {
            ResetTexts();
            FillTexts();
        }

        private void FillTexts()
        {
            if (groupClassSessionsData.CompleteGroupClassSession.Count == 0)
            {
                NoData();
                return;
            }

            var failsList = groupClassSessionsData.CompleteGroupClassSession.Where(x => x.TotalFails > 0)
                .OrderByDescending(x => x.TotalFails).ToList();
            var levelsList = groupClassSessionsData.CompleteGroupClassSession.Where(x => x.LevelsFinishedQuantity > 0)
                .OrderByDescending(x => x.LevelsFinishedQuantity).ToList();
            var playsMost = groupClassSessionsData.CompleteGroupClassSession.OrderByDescending(x => x.TotalElapsedTime)
                .FirstOrDefault();

            if (failsList.Count > 0)
            {
                var mostFails = failsList.FirstOrDefault();
                var lessFails = failsList.LastOrDefault();

                mostFailsText.text = mostFails.Student.Name + " - " + mostFails.TotalFails + " falha(s)";
                lessFailsText.text = lessFails.Student.Name + " - " + lessFails.TotalFails + " falha(s)";
            }
            else
            {
                mostFailsText.text = "Dados insuficientes";
                lessFailsText.text = "Dados insuficientes";
            }

            var totalLevels = 0;

            if (levelsList.Count > 0)
            {
                var mostLevels = levelsList.FirstOrDefault();
                var lessLevels = levelsList.LastOrDefault();

                #region CompletedLevels

                var anySession = groupClassSessionsData.CompleteGroupClassSession.FirstOrDefault();
                totalLevels = anySession.LevelsFinishedQuantity + anySession.LevelsPendingQuantity;

                var count = 0;

                foreach (var session in groupClassSessionsData.CompleteGroupClassSession)
                    if (session.LevelsFinishedQuantity == totalLevels)
                        count++;

                if (count == groupClassSessionsData.CompleteGroupClassSession.Count)
                {
                    mostCompletedLevels.text = "Todas as fases concluídas";
                    lessLevelsCompleted.text = "Todas as fases concluídas";
                }
                else
                {
                    mostCompletedLevels.text =
                        mostLevels.Student.Name + " - " + mostLevels.LevelsFinishedQuantity + " fase(s)";
                    lessLevelsCompleted.text =
                        lessLevels.Student.Name + " - " + lessLevels.LevelsFinishedQuantity + " fase(s)";
                }

                #endregion
            }
            else
            {
                mostCompletedLevels.text = "Dados insuficientes";
                lessLevelsCompleted.text = "Dados insuficientes";
            }

            if (playsMost == null)
                playsTheMost.text = "Dados insuficientes";
            else
                playsTheMost.text = playsMost.Student.Name + " - " +
                                    (Mathf.CeilToInt(playsMost.TotalElapsedTime / 60) == 0
                                        ? 1
                                        : Mathf.CeilToInt(playsMost.TotalElapsedTime / 60)) + " minuto(s)";


            #region PieChart

            if (totalLevels == 0)
            {
                completedLevelsText.text = "Dados insuficientes";
                completedLevelsPieChart.DataSource.StartBatch();
                completedLevelsPieChart.DataSource.Clear();
                completedLevelsPieChart.DataSource.EndBatch();
                return;
            }

            var averageOfCompletedLevels = groupClassSessionsData.CompleteGroupClassSession.Select(x => x.LevelsFinishedQuantity).Average();
            
            completedLevelsText.text = ((float)averageOfCompletedLevels).ToString("0.0") + " / " + (levelsData.LevelsOfThisGroupClass.Count + levelsData.ActiveQuizesOfThisTeacher.Count);

            FillPieChart((float)averageOfCompletedLevels, totalLevels - (float)averageOfCompletedLevels);

            #endregion
        }

        private void FillPieChart(float currentValue, float maxValue)
        {
            completedLevelsPieChart.DataSource.StartBatch();
            completedLevelsPieChart.DataSource.Clear();

            completedLevelsPieChart.DataSource.AddCategory("NotCompleted", blackMat);
            completedLevelsPieChart.DataSource.SetValue("NotCompleted", maxValue);

            completedLevelsPieChart.DataSource.AddCategory("Completed", blueMat);
            completedLevelsPieChart.DataSource.SetValue("Completed", currentValue);

            completedLevelsPieChart.DataSource.EndBatch();
        }

        private void ResetTexts()
        {
            mostFailsText.text = "";
            lessFailsText.text = "";
            playsTheMost.text = "";
            mostCompletedLevels.text = "";
            lessLevelsCompleted.text = "";
        }

        private void NoData()
        {
            mostFailsText.text = "Dados insuficientes";
            lessFailsText.text = "Dados insuficientes";
            playsTheMost.text = "Dados insuficientes";
            mostCompletedLevels.text = "Dados insuficientes";
            lessLevelsCompleted.text = "Dados insuficientes";

            completedLevelsText.text = "Dados insuficientes";
            completedLevelsPieChart.DataSource.StartBatch();
            completedLevelsPieChart.DataSource.Clear();
            completedLevelsPieChart.DataSource.EndBatch();
        }
    }
}