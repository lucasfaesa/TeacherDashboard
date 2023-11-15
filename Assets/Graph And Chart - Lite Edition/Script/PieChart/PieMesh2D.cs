using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     pie mesh creation class
    /// </summary>
    internal class PieMesh
    {
        public static void Generate2dPath(List<Vector2> path, List<int> tringles, float startAngle, float angleSpan,
            float radius, float innerRadius, int segments)
        {
            var segmentAngle = angleSpan / segments;
            var currentAngle = startAngle;

            path.Clear();
            for (var i = 0; i <= segments; i++)
            {
                path.Add(Vector2.zero);
                path.Add(Vector2.zero);
            }

            for (var i = 0; i <= segments; i++)
            {
                currentAngle += segmentAngle;
                var cos = Mathf.Cos(currentAngle);
                var sin = Mathf.Sin(currentAngle);

                var innerVertex = new Vector3(cos * innerRadius, sin * innerRadius, 0f);
                var outerVertex = new Vector3(cos * radius, sin * radius, 0f);
                path[i] = innerVertex;
                path[path.Count - 1 - i] = outerVertex;
            }

            for (var i = 1; i <= segments; i++)
            {
                tringles.Add(i - 1);
                tringles.Add(i);
                tringles.Add(path.Count - i);

                tringles.Add(i);
                tringles.Add(path.Count - i);
                tringles.Add(path.Count - 1 - i);
            }
        }

        public static void Generate2dMesh(IChartMesh mesh, float startAngle, float angleSpan, float radius,
            float innerRadius, int segments)
        {
            var segmentAngle = angleSpan / segments;
            var currentAngle = startAngle;
            var segmenUv = 1f / segments;
            var currentUv = 0f;
            var cos = Mathf.Cos(currentAngle);
            var sin = Mathf.Sin(currentAngle);

            var prevInnerVertex = ChartCommon.CreateVertex(new Vector3(cos * innerRadius, sin * innerRadius, 0f),
                new Vector2(currentUv, 0f));
            var prevOuterVertex =
                ChartCommon.CreateVertex(new Vector3(cos * radius, sin * radius, 0f), new Vector2(currentUv, 1f));
            for (var i = 1; i < segments + 1; i++)
            {
                currentUv += segmenUv;
                currentAngle += segmentAngle;
                cos = Mathf.Cos(currentAngle);
                sin = Mathf.Sin(currentAngle);

                var innerVertex = ChartCommon.CreateVertex(new Vector3(cos * innerRadius, sin * innerRadius, 0f),
                    new Vector2(currentUv, 0f));
                var outerVertex = ChartCommon.CreateVertex(new Vector3(cos * radius, sin * radius, 0f),
                    new Vector2(currentUv, 1f));
                mesh.AddQuad(prevInnerVertex, innerVertex, prevOuterVertex, outerVertex);
                prevInnerVertex = innerVertex;
                prevOuterVertex = outerVertex;
            }
        }
    }
}