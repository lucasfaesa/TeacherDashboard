﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    /// <summary>
    ///     generates a pie mesh for use with canvas charts
    /// </summary>
    public class PieCanvasGenerator : Image, IPieGenerator, ICanvasRaycastFilter
    {
        private bool mPopulated;
        private float mStartAngle;
        private float mAngleSpan;
        private float mRadius;
        private float mInnerRadius;
        private int mSegements;

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
            PieMesh.Generate2dMesh(mesh, mStartAngle, mAngleSpan, mRadius, mInnerRadius, mSegements);
        }

        public void Generate(float startAngle, float angleSpan, float radius, float innerRadius, int segments,
            float outerdepth, float innerdepth)
        {
            var maxRad = Mathf.Max(radius, innerRadius) * 2f;
            rectTransform.sizeDelta = new Vector2(maxRad, maxRad);
            mPopulated = true;
            mStartAngle = startAngle;
            mAngleSpan = angleSpan;
            mRadius = radius;
            mInnerRadius = innerRadius;
            mSegements = segments;
            SetAllDirty();
            Rebuild(CanvasUpdate.PreRender);
        }

        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out localPoint);

            var sqrMag = localPoint.sqrMagnitude;
            var maxRad = mRadius;
            var minRad = mInnerRadius;

            if (mRadius < mInnerRadius)
            {
                maxRad = mInnerRadius;
                minRad = mRadius;
            }

            if (sqrMag > maxRad * maxRad)
                return false;
            if (sqrMag < minRad * minRad)
                return false;
            var angle = Mathf.Atan2(localPoint.y, localPoint.x);
            var delta = angle - mStartAngle;
            delta -= Mathf.Floor(delta / (Mathf.PI * 2f)) * Mathf.PI * 2f;
            if (delta > mAngleSpan)
                return false;
            return true;
        }
    }
}