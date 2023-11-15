using System;
using System.Collections.Generic;
using System.IO;
using ChartAndGraph;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class LargeDataFeed : MonoBehaviour, IComparer<DoubleVector2>
{
    public string Category = "Player 1";
    public int DownSampleToPoints = 100;
    private double currentPagePosition;
    private double currentZoom;
    private GraphChartBase graph;
    private double mCurrentPageSizeFactor = double.NegativeInfinity;
    private List<DoubleVector2> mData = new(); // the data held by the chart
    private readonly double pageSize = 2f;

    private double PageSizeFactor => pageSize * graph.DataSource.HorizontalViewSize;

    private void Start()
    {
        graph = GetComponent<GraphChartBase>();
        SetInitialData();
    }

    public void Update()
    {
        if (graph != null)
        {
            //check the scrolling position of the graph. if we are past the view size , load a new page
            var pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            var pageEndThreshold = currentPagePosition + mCurrentPageSizeFactor - graph.DataSource.HorizontalViewSize;

            if (graph.HorizontalScrolling < pageStartThreshold || graph.HorizontalScrolling > pageEndThreshold ||
                currentZoom >= graph.DataSource.HorizontalViewSize * 2f)
            {
                currentZoom = graph.DataSource.HorizontalViewSize;
                mCurrentPageSizeFactor = PageSizeFactor * 0.9f;
                LoadPage(graph.HorizontalScrolling);
            }
        }
    }


    public int Compare(DoubleVector2 x, DoubleVector2 y)
    {
        if (x.x < y.x)
            return -1;
        if (x.x > y.x)
            return 1;
        return 0;
    }

    public DoubleVector2 GetLastPoint()
    {
        if (mData.Count == 0)
            return new DoubleVector2();
        return mData[mData.Count - 1];
    }

    /// <summary>
    ///     called with Start(). These will be used to load the data into the large data feed. You should replace this with
    ///     your own loading logic.
    /// </summary>
    private void SetInitialData()
    {
        var data = new List<DoubleVector2>(250000);
        double x = 0f;
        double y = 200f;
        for (var i = 0; i < 25000; i++) // initialize with random data
        {
            data.Add(new DoubleVector2(x, y));
            y += Random.value * 10f - 5f;
            x += Random.value;
        }

        SetData(data);
    }

    public void SaveToFile(string path)
    {
        using (var file = new StreamWriter(path))
        {
            file.WriteLine(mData.Count);
            for (var i = 0; i < mData.Count; i++)
            {
                var item = mData[i];
                file.WriteLine(item.x);
                file.WriteLine(item.y);
            }
        }
    }

    public void LoadFromFile(string path)
    {
        try
        {
            var data = new List<DoubleVector2>();
            using (var file = new StreamReader(path))
            {
                var count = int.Parse(file.ReadLine());
                for (var i = 0; i < count; i++)
                {
                    var x = double.Parse(file.ReadLine());
                    var y = double.Parse(file.ReadLine());
                    data.Add(new DoubleVector2(x, y));
                }
            }

            SetData(data);
        }
        catch (Exception)
        {
            throw new Exception("Invalid file format");
        }
    }

    /// <summary>
    ///     vertify's that the graph data is sorted so it can be searched using a binary search.
    /// </summary>
    /// <returns></returns>
    private bool VerifySorted(List<DoubleVector2> data)
    {
        if (data == null)
            return true;
        for (var i = 1; i < data.Count; i++)
            if (data[i].x < data[i - 1].x)
                return false;
        return true;
    }

    partial void OnDataLoaded();

    /// <summary>
    ///     set the data of the large data graph
    /// </summary>
    /// <param name="data"></param>
    public void SetData(List<DoubleVector2> data)
    {
        if (data == null)
            data = new List<DoubleVector2>(); // set up an empty list instead of null
        if (VerifySorted(data) == false)
        {
            Debug.LogWarning(
                "The data used with large data feed must be sorted acoording to the x value, aborting operation");
            return;
        }

        mData = data;
        OnDataLoaded();
        LoadPage(currentPagePosition); // load the page at position 0
    }

    private int
        FindClosestIndex(
            double position) // if you want to know what is index is currently displayed . use binary search to find it
    {
        //NOTE :: this method assumes your data is sorted !!! 
        var res = mData.BinarySearch(new DoubleVector2(position, 0.0), this);
        if (res >= 0)
            return res;
        return ~res;
    }

    private void
        findPointsForPage(double position, out int start,
            out int end) // given a page position , find the right most and left most indices in the data for that page. 
    {
        var index = FindClosestIndex(
            position); // use binary search to find the closest position to the current scroll point

        var endPosition = position + PageSizeFactor;
        var startPosition = position - PageSizeFactor;

        //starting from the current index , we find the page boundries
        for (start = index; start > 0; start--)
            if (mData[start].x <
                startPosition) // take the first point that is out of the page. so the graph doesn't break at the edge
                break;

        for (end = index; end < mData.Count; end++)
            if (mData[end].x > endPosition) // take the first point that is out of the page
                break;
    }

    private void LoadWithoutDownSampling(int start, int end)
    {
        for (var i = start; i < end; i++) // load the data
            graph.DataSource.AddPointToCategory(Category, mData[i].x, mData[i].y);
    }

    private void LoadWithDownSampling(int start, int end)
    {
        var total = end - start;

        if (DownSampleToPoints >= total)
        {
            LoadWithoutDownSampling(start, end);
            return;
        }

        var sampleCount = total / (double)DownSampleToPoints;
        // graph.DataSource.AddPointToCategory(Category, mData[start].x, mData[start].y);
        for (var i = 0; i < DownSampleToPoints; i++)
        {
            var fractionStart = start + (int)(i * sampleCount); // the first point with a fraction
            var fractionEnd = start + (int)((i + 1) * sampleCount); // the first point with a fraction
            fractionEnd = Math.Min(fractionEnd, mData.Count - 1);
            double x = 0, y = 0;
            var divide = 0.0;
            for (var j = fractionStart; j < fractionEnd; j++) // avarge the points
            {
                x += mData[j].x;
                y += mData[j].y;
                divide++;
            }

            if (divide > 0.0)
            {
                x /= divide;
                y /= divide;
                graph.DataSource.AddPointToCategory(Category, x, y);
            }
            else
            {
                Debug.Log("error");
            }
        }
        //   graph.DataSource.AddPointToCategory(Category, mData[last].x, mData[last].y);
    }

    private void LoadPage(double pagePosition)
    {
        if (graph != null)
        {
            Debug.Log("Loading page :" + pagePosition);
            graph.DataSource.StartBatch(); // call start batch 
            graph.DataSource.HorizontalViewOrigin = 0;
            int start, end;
            findPointsForPage(pagePosition, out start, out end); // get the page edges
            graph.DataSource.ClearCategory(Category); // clear the cateogry

            if (DownSampleToPoints <= 0)
                LoadWithoutDownSampling(start, end);
            else
                LoadWithDownSampling(start, end);
            graph.DataSource.EndBatch();
            graph.HorizontalScrolling = pagePosition;
        }

        currentPagePosition = pagePosition;
    }
}