using System;
using _GraphsAndCharts.Scripts.StudentSpaceship;
using ChartAndGraph;
using UnityEngine;

namespace _GraphsAndCharts.Scripts
{
    [CreateAssetMenu(fileName = "HoverChartsData", menuName = "ScriptableObjects/Graphs/HoverChartsData")]
    public class HoverChartsDataSO : ScriptableObject
    {
        [SerializeField] private StudentSpaceshipDataSO studentSpaceshipData;
        public event Action<HoverDataDisplay> OnHoverEnter;

        public event Action OnHoverExit;

        public void HoverEnter(BarChart.BarEventArgs args)
        {
            var data = new HoverDataDisplay(args.Category, Mathf.RoundToInt((float)args.Value).ToString());
            OnHoverEnter?.Invoke(data);
        }

        public void HoverEnter(PieChart.PieEventArgs args)
        {
            var data = new HoverDataDisplay(args.Category, Mathf.RoundToInt((float)args.Value).ToString());
            OnHoverEnter?.Invoke(data);
        }
        
        public void HoverEnter(GraphChart.GraphEventArgs args)
        {
            var data = new HoverDataDisplay("",  $"{args.Value.y} Pontos\n" +
                        $"{studentSpaceshipData.CurrentSpaceshipSessionOfGraph[(int)args.Value.x].PlayedDate:dd/MM/yy} ");
            OnHoverEnter?.Invoke(data);
        }

        public void HoverExit()
        {
            OnHoverExit?.Invoke();
        }
    }

    public class HoverDataDisplay
    {
        public string info;
        public string value;

        public HoverDataDisplay(string infos, string val)
        {
            info = infos;
            value = val;
        }
    }
}