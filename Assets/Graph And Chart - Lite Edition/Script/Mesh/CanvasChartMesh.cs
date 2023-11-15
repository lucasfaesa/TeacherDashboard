using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    /// <summary>
    ///     a chart mesh that is used for canvas rendering
    /// </summary>
    internal class CanvasChartMesh : ChartMeshBase
    {
        private List<UIVertex> mListWrapAround;
        private readonly bool mTextOnly;
        private readonly UIVertex[] mTmpQuad = new UIVertex[4];
        private VertexHelper mVHWrapAround;

        public CanvasChartMesh(bool forText)
        {
            mTextOnly = forText;
        }

        public CanvasChartMesh(VertexHelper wrapAround)
        {
            mVHWrapAround = wrapAround;
        }

        public CanvasChartMesh(List<UIVertex> wrapAround)
        {
            mListWrapAround = wrapAround;
        }

        public void WrapAround(VertexHelper wrapAround)
        {
            Clear();
            mVHWrapAround = wrapAround;
            mListWrapAround = null;
        }

        public void WrapAround(List<UIVertex> wrapAround)
        {
            Clear();
            mVHWrapAround = null;
            mListWrapAround = wrapAround;
        }

        public override BillboardText AddText(AnyChart chart, MonoBehaviour prefab, Transform parentTransform,
            int fontSize, float fontScale, string text, float x, float y, float z, float angle, object userData)
        {
            if (mTextOnly)
                return base.AddText(chart, prefab, parentTransform, fontSize, fontScale, text, x, y, z, angle,
                    userData);
            return null;
        }

        private UIVertex FloorVertex(UIVertex vertex)
        {
            var newPosition = new Vector3(Mathf.Floor(vertex.position.x), Mathf.Floor(vertex.position.y),
                Mathf.Floor(vertex.position.z));
            vertex.position = newPosition;
            return vertex;
        }

        public override void AddQuad(UIVertex vLeftTop, UIVertex vRightTop, UIVertex vLeftBottom, UIVertex vRightBottom)
        {
            if (mListWrapAround != null)
            {
                mListWrapAround.Add(vLeftTop);
                mListWrapAround.Add(vRightTop);
                mListWrapAround.Add(vRightBottom);
                mListWrapAround.Add(vLeftBottom);
                return;
            }

            if (mVHWrapAround != null)
            {
                mTmpQuad[0] = vLeftTop;
                mTmpQuad[1] = vRightTop;
                mTmpQuad[2] = vRightBottom;
                mTmpQuad[3] = vLeftBottom;
                mVHWrapAround.AddUIVertexQuad(mTmpQuad);
            }
        }

        public override void AddXYRect(Rect rect, int subMeshGroup, float depth)
        {
            var uvs = GetUvs(rect);
            var leftTop = ChartCommon.CreateVertex(new Vector3(rect.xMin, rect.yMin, depth), uvs[0]);
            var rightTop = ChartCommon.CreateVertex(new Vector3(rect.xMax, rect.yMin, depth), uvs[1]);
            var leftBottom = ChartCommon.CreateVertex(new Vector3(rect.xMin, rect.yMax, depth), uvs[2]);
            var rightBottom = ChartCommon.CreateVertex(new Vector3(rect.xMax, rect.yMax, depth), uvs[3]);
            AddQuad(leftTop, rightTop, leftBottom, rightBottom);
        }

        public override void AddXZRect(Rect rect, int subMeshGroup, float yPosition)
        {
            // this does nothing , canvas are 2d only
        }

        public override void AddYZRect(Rect rect, int subMeshGroup, float xPosition)
        {
            // this does nothing , canvas are 2d only
        }
    }
}