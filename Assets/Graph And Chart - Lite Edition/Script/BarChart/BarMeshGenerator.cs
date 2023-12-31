﻿using UnityEngine;

namespace ChartAndGraph
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    internal class BarMeshGenerator : BarGenerator
    {
        /// <summary>
        ///     Sets the dimention of the generated mesh. can be either 2d or 3d
        /// </summary>
        public MeshDimention MeshDimention = MeshDimention._2D;

        /// <summary>
        ///     Sets the way materials are fit to the bar mesh
        /// </summary>
        public BarMaterialFit MaterialFit = BarMaterialFit.Stretch;

        /// <summary>
        ///     Contains a mesh that was generate for this object only and should be destoryed once the object is cleaned
        /// </summary>
        private Mesh mCleanMesh;

        private MeshDimention mCurrentDimention;
        private BarMaterialFit mCurrentMaterialFit;

        /// <summary>
        ///     the mesh filter for this object
        /// </summary>
        private MeshFilter mFilter;

        private void OnDestroy()
        {
            Clear();
        }

        private bool EnsureMeshFilter()
        {
            if (mFilter == null)
                mFilter = GetComponent<MeshFilter>();
            if (mFilter == null)
                return false;
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="normalizedSize"></param>
        public override void Generate(float normalizedSize, float scale)
        {
            if (EnsureMeshFilter() == false) // No mesh filter attached
                return;

            if (mFilter.sharedMesh != null)
                if (MaterialFit == BarMaterialFit.Trim && mCurrentMaterialFit == BarMaterialFit.Trim)
                    if (MeshDimention == mCurrentDimention)
                    {
                        if (MeshDimention == MeshDimention._2D)
                            BarMesh.Update2DMeshUv(mFilter.sharedMesh, normalizedSize);
                        else
                            BarMesh.Update3DMeshUv(mFilter.sharedMesh, normalizedSize);
                        return;
                    }

            mCurrentDimention = MeshDimention;
            mCurrentMaterialFit = MaterialFit;
            if (MaterialFit == BarMaterialFit.Stretch)
            {
                if (MeshDimention == MeshDimention._2D)
                    mFilter.sharedMesh = BarMesh.StrechMesh2D;
                else
                    mFilter.sharedMesh = BarMesh.StrechMesh3D;
                ChartCommon.CleanMesh(null, ref mCleanMesh);
                return;
            }

            if (MaterialFit == BarMaterialFit.Trim)
            {
                Mesh newMesh = null;
                if (MeshDimention == MeshDimention._2D)
                    newMesh = BarMesh.Generate2DMesh(normalizedSize);
                else
                    newMesh = BarMesh.Generate3DMesh(normalizedSize);
                mFilter.sharedMesh = newMesh;
                ChartCommon.CleanMesh(newMesh, ref mCleanMesh);
            }
            else
            {
            }
        }

        /// <summary>
        /// </summary>
        public override void Clear()
        {
            ChartCommon.CleanMesh(null, ref mCleanMesh);
        }
    }
}