using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using iShape.Geometry.Container;
using iShape.Geometry;
using iShape.Mesh2d;
using iShape.Triangulation.Shape.Delaunay;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// Class to create a tessellated mesh from the given points.
    /// </summary>
    public sealed class TessellatedMeshCreator
    {
        /// <summary>
        /// Convert the given points to a PlainShape.
        /// </summary>
        /// <param name="iGeom"></param>
        /// <param name="allocator"></param>
        /// <param name="hull">Area outline points</param>
        /// <param name="holes">hole outline points</param>
        /// <returns></returns>
        public PlainShape ConvertToPlainShape(IntGeom iGeom, Allocator allocator, Vector2[] hull, Vector2[][] holes)
        {
            var iHull = iGeom.Int(hull);
            iHull = RemoveDuplicates(iHull);

            IntShape iShape;
            if (holes != null && holes.Length > 0)
            {
                var iHoles = iGeom.Int(holes);
                iShape = new IntShape(iHull, iHoles);
            }
            else
            {
                iShape = new IntShape(iHull, Array.Empty<IntVector[]>());
            }

            var pShape = new PlainShape(iShape, allocator);

            return pShape;
        }


        /// <summary>
        /// delete duplicate points
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        IntVector[] RemoveDuplicates(IntVector[] vectors)
        {
            List<IntVector> uniqueList = new List<IntVector>();
            HashSet<IntVector> seen = new HashSet<IntVector>();

            foreach (IntVector vector in vectors)
            {
                if (!seen.Contains(vector))
                {
                    seen.Add(vector);
                    uniqueList.Add(vector);
                }
            }

            return uniqueList.ToArray();
        }


        /// <summary>
        /// Create a mesh from the given points and tessellate it.
        /// </summary>
        /// <param name="points">Mesh vertice points. They must be in counterclockwise order.</param>
        /// <param name="meshFilter"></param>
        /// <param name="tessellateMaxEdge"></param>
        /// <param name="tessellateMaxArea"></param>
        public void CreateTessellatedMesh(List<Vector3> points, MeshFilter meshFilter, float tessellateMaxEdge, float tessellateMaxArea)
        {
            var iGeom = IntGeom.DefGeom;
            
            Vector2[] hull = new Vector2[points.Count];

            int index = 0;
            foreach (var point in points)
            {
                hull[index] = new Vector2(point.x, point.z);
                index++;
            }
            
            var pShape = ConvertToPlainShape(iGeom, Allocator.Temp, hull, null);
            
            var extraPoints = new NativeArray<IntVector>(0, Allocator.Temp);
            var delaunay = pShape.Delaunay(iGeom.Int(tessellateMaxEdge), extraPoints, Allocator.Temp);

            delaunay.Tessellate(iGeom, tessellateMaxArea);

            extraPoints.Dispose();

            var triangles = delaunay.Indices(Allocator.Temp);
            var vertices = delaunay.Vertices(Allocator.Temp, iGeom, 0);

            delaunay.Dispose();

            // set each triangle as a separate mesh
            var subVertices = new NativeArray<float3>(3, Allocator.Temp);
            var subIndices = new NativeArray<int>(new[] { 0, 1, 2 }, Allocator.Temp);

            var colorMesh = new NativeColorMesh(triangles.Length, Allocator.Temp);

            
            for (int i = 0; i < triangles.Length; i += 3)
            {

                for (int j = 0; j < 3; j += 1)
                {
                    var v = vertices[triangles[i + j]];
                    subVertices[j] = new float3(v.x, v.z, v.y);
                }

                var subMesh = new StaticPrimitiveMesh(subVertices, subIndices, Allocator.Temp);
                var color = Color.white;

                colorMesh.AddAndDispose(subMesh, color);
            }

            // Create a new Unity Mesh
            Mesh mesh = new Mesh();

            subIndices.Dispose();
            subVertices.Dispose();

            vertices.Dispose();
            triangles.Dispose();
            colorMesh.FillAndDispose(mesh);

            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
            pShape.Dispose();
        }
    }
}
