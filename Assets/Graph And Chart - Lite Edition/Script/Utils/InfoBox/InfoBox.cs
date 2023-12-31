﻿using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    /// <summary>
    ///     this class demonstrates the use of chart events
    /// </summary>
    public partial class InfoBox : MonoBehaviour
    {
        public PieChart[] PieChart;
        public BarChart[] BarChart;
        public GraphChartBase[] GraphChart;
        public RadarChart[] RadarChart;
        public PyramidChart[] PyramidChart;
        public Text infoText;

        // Use this for initialization
        private void Start()
        {
            HookChartEvents();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void BarHovered(BarChart.BarEventArgs args)
        {
            infoText.text = string.Format("({0},{1}) : {2}", args.Category, args.Group, args.Value);
        }

        private void RadarHovered(RadarChart.RadarEventArgs args)
        {
            infoText.text = string.Format("{0},{1} : {2}", args.Category, args.Group,
                ChartAdancedSettings.Instance.FormatFractionDigits(2, args.Value));
        }

        private void GraphClicked(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("{0} : {1},{2} Clicked", args.Category, args.XString, args.YString);
            else
                infoText.text = string.Format("{0} : {1},{2} : Sample Size {3} Clicked", args.Category, args.XString,
                    args.YString, args.Magnitude);
        }

        private void GraphHoverd(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("{0} : {1},{2}", args.Category, args.XString, args.YString);
            else
                infoText.text = string.Format("{0} : {1},{2} : Sample Size {3}", args.Category, args.XString,
                    args.YString, args.Magnitude);
        }

        private void GraphLineClicked(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("Line Start at {0} : {1},{2} Clicked", args.Category, args.XString,
                    args.YString);
            else
                infoText.text = string.Format("Line Start at{0} : {1},{2} : Sample Size {3} Clicked", args.Category,
                    args.XString, args.YString, args.Magnitude);
        }

        private void GraphLineHoverd(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("Line Start at {0} : {1},{2}", args.Category, args.XString, args.YString);
            else
                infoText.text = string.Format("Line Start at {0} : {1},{2} : Sample Size {3}", args.Category,
                    args.XString, args.YString, args.Magnitude);
        }

        private void PieHovered(PieChart.PieEventArgs args)
        {
            infoText.text = string.Format("{0} : {1}", args.Category, args.Value);
        }

        private void PyramidHovered(PyramidChart.PyramidEventArgs args)
        {
            infoText.text = string.Format("{0} : {1}", args.Title, args.Text);
        }


        private void NonHovered()
        {
            infoText.text = "";
        }

        partial void HookCandle();

        public void HookChartEvents()
        {
            if (PieChart != null)
                foreach (var pie in PieChart)
                {
                    if (pie == null)
                        continue;
                    pie.PieHovered.AddListener(PieHovered); // add listeners for the pie chart events
                    pie.NonHovered.AddListener(NonHovered);
                }

            if (PyramidChart != null)
                foreach (var pyramid in PyramidChart)
                {
                    if (pyramid == null)
                        continue;
                    pyramid.ItemHovered.AddListener(PyramidHovered); // add listeners for the pie chart events
                    pyramid.NonHovered.AddListener(NonHovered);
                }

            if (BarChart != null)
                foreach (var bar in BarChart)
                {
                    if (bar == null)
                        continue;
                    bar.BarHovered.AddListener(BarHovered); // add listeners for the bar chart events
                    bar.NonHovered.AddListener(NonHovered);
                }

            if (GraphChart != null)
                foreach (var graph in GraphChart)
                {
                    if (graph == null)
                        continue;
                    graph.PointClicked.AddListener(GraphClicked);
                    graph.PointHovered.AddListener(GraphHoverd);
                    if (graph is GraphChart)
                    {
                        ((GraphChart)graph).LineClicked.AddListener(GraphLineClicked);
                        ((GraphChart)graph).LineHovered.AddListener(GraphLineHoverd);
                    }

                    graph.NonHovered.AddListener(NonHovered);
                }

            HookCandle();
            if (RadarChart != null)
                foreach (var radar in RadarChart)
                {
                    if (radar == null)
                        continue;
                    radar.PointHovered.AddListener(RadarHovered);
                    radar.NonHovered.AddListener(NonHovered);
                }
        }
    }
}