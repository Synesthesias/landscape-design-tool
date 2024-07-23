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
        public GameObject GenerateWall(Mesh originalMesh, float wallHeight, Vector3 wallDirection, Material wallMaterial)
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

            List<Vector3> wallVertices = new List<Vector3>();
            List<int> wallTriangles = new List<int>();
            List<Edge> outlineEdges = new List<Edge>();

            //Get outline edges
            foreach (var item in edgeCount)
            {
                if (item.Value == 1)
                {
                    outlineEdges.Add(item.Key);
                }
            }

            // Set the first edge as the starting point
            List<Vector3> SortedOutlineVertices = new List<Vector3>
            {
                outlineEdges[0].vertex1,
                outlineEdges[0].vertex2
            };
            outlineEdges.RemoveAt(0);

            // Search and sort adjacent edges
            for (int i = 0; outlineEdges.Count != 0; i++)
            {
                Vector3 preEndPoint = SortedOutlineVertices[i * 2 + 1];
                for(int j = 0; j < outlineEdges.Count; j++)
                {
                    if (outlineEdges[j].vertex1 == preEndPoint)
                    {
                        SortedOutlineVertices.Add(outlineEdges[j].vertex1);
                        SortedOutlineVertices.Add(outlineEdges[j].vertex2);
                        outlineEdges.RemoveAt(j);
                        break;
                    }
                    else if (outlineEdges[j].vertex2 == preEndPoint)
                    {
                        SortedOutlineVertices.Add(outlineEdges[j].vertex2);
                        SortedOutlineVertices.Add(outlineEdges[j].vertex1);
                        outlineEdges.RemoveAt(j);
                        break;
                    }
                }
            }

            // Set wall vertices, triangles and uv
            Vector2[] uv = new Vector2[SortedOutlineVertices.Count * 2];
            float uvXPitch = 1.0f / SortedOutlineVertices.Count;

            for(int i = 0; i * 2 < SortedOutlineVertices.Count; i++)
            {
                Vector3 v1 = SortedOutlineVertices[i * 2];
                Vector3 v2 = SortedOutlineVertices[i * 2 + 1];

                wallVertices.Add(v1);
                wallVertices.Add(v2);
                wallVertices.Add(v1 + wallDirection.normalized * wallHeight);
                wallVertices.Add(v2 + wallDirection.normalized * wallHeight);

                uv[i * 4] = new Vector2(i * 2 * uvXPitch, 1);
                uv[i * 4 + 1] = new Vector2((i * 2 + 1) * uvXPitch, 1);
                uv[i * 4 + 2] = new Vector2(i * 2 * uvXPitch, 0);
                uv[i * 4 + 3] = new Vector2((i * 2 + 1) * uvXPitch, 0);

                int count = wallVertices.Count;
                wallTriangles.Add(count - 4);
                wallTriangles.Add(count - 3);
                wallTriangles.Add(count - 2);
                wallTriangles.Add(count - 3);
                wallTriangles.Add(count - 1);
                wallTriangles.Add(count - 2);
            }

            Mesh wallMesh = new Mesh();
            wallMesh.vertices = wallVertices.ToArray();
            wallMesh.triangles = wallTriangles.ToArray();
            wallMesh.uv = uv;

            GameObject wallObject = new GameObject("Wall");
            MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = wallObject.AddComponent<MeshRenderer>();
            meshFilter.mesh = wallMesh;
            meshRenderer.material = wallMaterial;

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
            Edge edge = new Edge(vertices[v1], vertices[v2]);
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
            public Vector3 vertex1;
            public Vector3 vertex2;

            public Edge(Vector3 vertex1, Vector3 vertex2)
            {
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

