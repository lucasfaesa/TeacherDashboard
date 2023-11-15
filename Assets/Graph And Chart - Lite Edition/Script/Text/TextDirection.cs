using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    [ExecuteInEditMode]
    public class TextDirection : MonoBehaviour
    {
        public Material PointMaterial;
        public Material LineMaterial;

        public float Length = 20f;
        public float Gap = 5f;
        public float Thickness = 2f;
        public float PointSize = 10f;

        public CanvasLines Lines;
        public CanvasLines Point;
        public MonoBehaviour Text;
        private TextController controller;
        private Transform relativeFrom;

        private Transform relativeTo;

        public void Start()
        {
        }

        public void LateUpdate()
        {
            if (relativeFrom == null || relativeTo == null || controller == null || controller.Camera == null)
                return;
            var dir = (relativeTo.position - relativeFrom.position).normalized * Length;
            var inverse = Quaternion.Inverse(controller.Camera.transform.rotation);
            dir = inverse * dir;
            SetDirection(dir);
        }

        public void SetTextController(TextController control)
        {
            controller = control;
        }

        public void SetRelativeTo(Transform from, Transform to)
        {
            relativeTo = to;
            relativeFrom = from;
        }

        public void SetDirection(float angle)
        {
            SetDirection(ChartCommon.FromPolar(angle, Length));
        }

        private void SetDirection(Vector3 dir)
        {
            //Vector3 dir = ChartCommon.FromPolar(angle, Length);
            var sign = Mathf.Sign(dir.x);
            var dirAdd = new Vector3(1f, 0f, 0f) * sign * Length;
            var gapAdd = new Vector3(1f, 0f, 0f) * sign * Gap;
            if (LineMaterial != null)
            {
                var segments = new List<CanvasLines.LineSegement>();
                segments.Add(new CanvasLines.LineSegement(new[] { Vector3.zero, dir, dir + dirAdd }));
                Lines.Thickness = Thickness;
                Lines.Tiling = 1f;
                Lines.material = LineMaterial;
                Lines.SetLines(segments);
            }

            if (PointMaterial != null)
            {
                var segments = new List<CanvasLines.LineSegement>();
                segments.Add(new CanvasLines.LineSegement(new[] { Vector3.zero }));
                Point.MakePointRender(PointSize);
                Point.material = PointMaterial;
                Point.SetLines(segments);
            }

            var anchor = new Vector2(0.5f, 0.5f);
            var pivot = new Vector2(sign > 0f ? 0f : 1f, 0.5f);

            var rect = Text.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = pivot;
                var t = Text.GetComponent<Text>();
                if (t != null)
                    t.alignment = sign > 0f ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
                else
                    ChartCommon.DoTextSign(Text, sign);
                rect.anchoredPosition = dir + dirAdd + gapAdd;
            }
            else
            {
                Debug.LogWarning("Direction text must contain a rect transform");
            }
        }
    }
}