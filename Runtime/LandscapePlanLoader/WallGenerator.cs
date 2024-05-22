using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// Class to generate walls around the outer edges of a mesh
    /// </summary>
    public sealed class WallGenerator
    {
        /// <summary>
        /// Generate a wall from the given mesh.
        /// </summary>
        /// <param name="originalMesh"></param>
        /// <param name="wallHeight"></param>
        /// <param name="wallDirection"></param>
        /// <returns></returns>
        public GameObject GenerateWall(Mesh originalMesh, float wallHeight, Vector3 wallDirection)
        {
            Vector3[] vertices = originalMesh.vertices;
            int[] triangles = originalMesh.triangles;

            Dictionary<Edge, int> edgeCount = new Dictionary<Edge, int>();

            // Get all edges of triangles and count duplicates
            for (int i = 0; i < triangles.Length; i += 3)
            {
                CountEdge(edgeCount, vertices, triangles[i], triangles[i + 1]);
                CountEdge(edgeCount, vertices, triangles[i + 1], triangles[i + 2]);
                CountEdge(edgeCount, vertices, triangles[i + 2], triangles[i]);
            }

            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();

            // Generate wall from outer edges
            foreach (var kvp in edgeCount)
            {
                if (kvp.Value == 1) // Edges with no duplicates mean outer edge
                {
                    Edge edge = kvp.Key;
                    Vector3 v1 = vertices[edge.vertexIndex1];
                    Vector3 v2 = vertices[edge.vertexIndex2];

                    newVertices.Add(v1);
                    newVertices.Add(v2);
                    newVertices.Add(v1 + wallDirection.normalized * wallHeight);
                    newVertices.Add(v2 + wallDirection.normalized * wallHeight);

                    int count = newVertices.Count;
                    newTriangles.Add(count - 4);
                    newTriangles.Add(count - 3);
                    newTriangles.Add(count - 2);
                    newTriangles.Add(count - 3);
                    newTriangles.Add(count - 1);
                    newTriangles.Add(count - 2);
                }
            }

            Mesh wallMesh = new Mesh();
            wallMesh.vertices = newVertices.ToArray();
            wallMesh.triangles = newTriangles.ToArray();
            wallMesh.RecalculateNormals();

            GameObject wallObject = new GameObject("Wall");
            MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = wallObject.AddComponent<MeshRenderer>();
            meshFilter.mesh = wallMesh;

            return wallObject;
        }

        /// <summary>
        /// Count edge duplicates
        /// </summary>
        /// <param name="edgeCount"></param>
        /// <param name="vertices"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        void CountEdge(Dictionary<Edge, int> edgeCount, Vector3[] vertices, int v1, int v2)
        {
            Edge edge = new Edge(v1, v2, vertices[v1], vertices[v2]);
            if (edgeCount.ContainsKey(edge))
            {
                edgeCount[edge]++;
            }
            else
            {
                edgeCount[edge] = 1;
            }
        }

        struct Edge
        {
            public int vertexIndex1;
            public int vertexIndex2;
            public Vector3 vertex1;
            public Vector3 vertex2;

            public Edge(int v1, int v2, Vector3 vertex1, Vector3 vertex2)
            {
                vertexIndex1 = Mathf.Min(v1, v2);
                vertexIndex2 = Mathf.Max(v1, v2);
                this.vertex1 = vertex1;
                this.vertex2 = vertex2;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Edge))
                    return false;

                Edge other = (Edge)obj;
                return (vertex1 == other.vertex1 && vertex2 == other.vertex2) ||
                       (vertex1 == other.vertex2 && vertex2 == other.vertex1);
            }

            public override int GetHashCode()
            {
                return vertex1.GetHashCode() ^ vertex2.GetHashCode();
            }
        }
    }
}

