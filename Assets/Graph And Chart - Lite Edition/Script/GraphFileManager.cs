using System;
using System.IO;

namespace ChartAndGraph
{
    internal class GraphFileManager
    {
        public void SaveGraphDataToFile(string path, GraphChartBase graph)
        {
            IInternalGraphData data = graph.DataSource;
            using (var file = new StreamWriter(path))
            {
                foreach (var cat in data.Categories)
                {
                    file.WriteLine(cat.Name);
                    file.WriteLine(cat.Data.Count);
                    for (var i = 0; i < cat.Data.Count; i++)
                    {
                        var item = cat.Data[i];
                        file.WriteLine(item.x);
                        file.WriteLine(item.y);
                    }
                }
            }
        }

        public void LoadGraphDataFromFile(string path, GraphChartBase graph)
        {
            try
            {
                using (var file = new StreamReader(path))
                {
                    while (file.Peek() > 0)
                    {
                        var catName = file.ReadLine();
                        if (graph.DataSource.HasCategory(catName) == false)
                            throw new Exception("category does not exist in the graph");
                        var count = int.Parse(file.ReadLine());
                        for (var i = 0; i < count; i++)
                        {
                            var x = double.Parse(file.ReadLine());
                            var y = double.Parse(file.ReadLine());
                            graph.DataSource.AddPointToCategory(catName, x, y);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Invalid file format");
            }
        }
    }
}