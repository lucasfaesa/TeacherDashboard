using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    [Serializable]
    public class PyramidCanvasGenerator : MaskableGraphic, IPyramidGenerator
    {
        public RectTransform Container;
        public Text Title;
        public Text Info;
        public Image Sprite;

        private bool mPopulated;
        private float mBaseX1;
        private float mBaseX2;
        private float mBaseSize;
        private float mSlopLeft;
        private float mSlopeRight;
        private float mHeight;
        private float mInset;
        private float mStartV;
        private float mEndV;
        private CanvasGroup mGroup;
        private UIVertex[] vertices = new UIVertex[4];
        private Vector2[] normals = new Vector2[4];

        protected override void Start()
        {
            base.Start();
            mGroup = ChartCommon.EnsureComponent<CanvasGroup>(gameObject);
        }

        public GameObject ContainerObject
        {
            get
            {
                if (Container == null)
                    return null;
                return Container.gameObject;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (mGroup != null)
                mGroup.alpha = alpha;
        }

        public override Texture mainTexture => material.mainTexture;

        public void ApplyInfo(string title, string text, Sprite image, float scale)
        {
            if (Container != null)
            {
                var centtroid = vertices[0].position + vertices[1].position + vertices[2].position +
                                vertices[3].position;
                centtroid *= 0.25f;
                var upperSize = Math.Abs(vertices[2].position.x - vertices[3].position.x);
                var downSize = Math.Abs(vertices[0].position.x - vertices[1].position.x);
                var total = upperSize + downSize;
                var weight = (downSize - upperSize) / total;
                weight = -weight * 0.5f + 0.5f;
                weight = Mathf.Lerp(0.3f, 0.7f, weight);
                centtroid.y = Mathf.Lerp(vertices[0].position.y, vertices[2].position.y, weight);
                centtroid = new Vector3(centtroid.x / rectTransform.rect.width,
                    centtroid.y / rectTransform.rect.height);
                centtroid += new Vector3(0.5f, 0.5f, 0f);
                Container.anchorMax = centtroid;
                Container.anchorMin = centtroid;
                Container.anchoredPosition = new Vector2();
                Container.sizeDelta = new Vector2(Mathf.Abs(mBaseX1 - mBaseX2) - mInset * 4, mHeight - mInset * 2);
                Container.localScale = new Vector3(scale, scale, scale);
            }

            if (Title != null)
                Title.text = title;
            if (Info != null)
                Info.text = text;
            if (Sprite != null)
                Sprite.sprite = image;
        }

        public void Generate()
        {
            SetAllDirty();
            Rebuild(CanvasUpdate.PreRender);
        }

        public Vector3 GetTextPosition(PyramidChart.JustificationType justification, bool isBase)
        {
            var res = new Vector3();
            switch (justification)
            {
                case PyramidChart.JustificationType.LeftAligned:
                    if (isBase)
                        res = vertices[0].position;
                    else
                        res = (vertices[0].position + vertices[2].position) * 0.5f;
                    break;
                case PyramidChart.JustificationType.RightAligned:
                    if (isBase)
                        res = vertices[1].position;
                    else
                        res = (vertices[1].position + vertices[3].position) * 0.5f;
                    break;
                case PyramidChart.JustificationType.CenterAligned:
                    if (isBase)
                        res = (vertices[0].position + vertices[1].position) * 0.5f;
                    else
                        res = (vertices[0].position + vertices[1].position + vertices[2].position +
                               vertices[3].position) * 0.25f;
                    break;
            }

            return res;
        }

        public void GetUpperBase(out float baseX1, out float baseX2)
        {
            baseX1 = vertices[2].position.x + mBaseSize * 0.5f;
            baseX2 = vertices[3].position.x + mBaseSize * 0.5f;
        }

        public void SetParams(float baseX1, float baseX2, float baseSize, float slopLeft, float slopeRight,
            float height, float inset, float startV, float endV)
        {
            mPopulated = true;
            mBaseX1 = baseX1;
            mBaseX2 = baseX2;
            mBaseSize = baseSize;
            mSlopLeft = slopLeft;
            mSlopeRight = slopeRight;
            mHeight = height;
            mInset = inset;
            mStartV = startV;
            mEndV = endV;
            PyramidMesh.Generate2dMesh(vertices, normals, mBaseX1, mBaseX2, mBaseSize, mSlopLeft, mSlopeRight, mHeight,
                mStartV, mEndV);
        }

#pragma warning disable 0672

        protected override void OnFillVBO(List<UIVertex> vbo)
        {
            vbo.Clear();
            if (mPopulated == false)
                return;
            var mesh = new CanvasChartMesh(vbo);
            FillChartMesh(mesh);
        }

#pragma warning restore 0672

#if (!UNITY_5_2_0) && (!UNITY_5_2_1)
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (mPopulated == false)
                return;
            var mesh = new CanvasChartMesh(vh);
            FillChartMesh(mesh);
        }
#endif
#pragma warning disable 0672
#if !UNITY_2017_1_OR_NEWER
        protected override void OnPopulateMesh(Mesh m)
        {
            m.Clear();
            if (mPopulated == false)
                return;
            WorldSpaceChartMesh chartmesh = new WorldSpaceChartMesh(true);
            FillChartMesh(chartmesh);
            chartmesh.ApplyToMesh(m);
        }
#endif
#pragma warning restore 0672

        private void FillChartMesh(IChartMesh mesh)
        {
            var v1 = vertices[0];
            var v2 = vertices[1];
            var v3 = vertices[2];
            var v4 = vertices[3];

            v1.position += (Vector3)(normals[0] * mInset);
            v2.position += (Vector3)(normals[1] * mInset);
            v3.position += (Vector3)(normals[2] * mInset);
            v4.position += (Vector3)(normals[3] * mInset);

            Vector2 inter;
            if (ChartCommon.SegmentIntersection(v1.position, v3.position, v2.position, v4.position, out inter))
            {
                v3.position = inter;
                v4.position = inter;
            }

            mesh.AddQuad(v1, v2, v3, v4);
        }
    }
}