using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using UnityEngine;
using PlateauToolkit.Maps;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// The class to modify the area mesh of landscape plan to the target height along the ground.
    /// </summary>
    public sealed class LandscapePlanMeshModifier
    {
        /// <summary>
        /// Transform and modify the area mesh to the target height along the ground.
        /// 
        /// The object with the cityObjectType set to COT_TINRelief (tag for ground obj) must be placed
        /// at the above or below of the target mesh as specified by the dbf file.
        /// 
        /// </summary>
        /// <param name="mesh">Area mesh</param>
        /// <param name="targetHeight">Target height from ground</param>
        /// <param name="globalPos">Global Position of MeshObject</param>
        /// <returns>Result indicating if the modification was successful</returns>
        public bool TryModifyMeshToTargetHeight(Mesh mesh, float targetHeight, Vector3 globalPos)
        {
            Vector3[] vertices = mesh.vertices; //read all vertices data from mesh

            //modify the height of each vertice to match the height limit by using raycast
            for (int verticeIndex = 0; verticeIndex < vertices.Length; verticeIndex++)
            {
                Physics.queriesHitBackfaces = true;

                //search the ground object for above the vertice
                RaycastHit[] hitsAbove = Physics.RaycastAll(vertices[verticeIndex], Vector3.up, float.PositiveInfinity);
                RaycastHit? hit = FindGroundObj(hitsAbove);

                //if the ground object is found, go to the next vertice
                if (hit != null)
                {
                    vertices[verticeIndex] = new Vector3(
                                vertices[verticeIndex].x,
                                hit.Value.point.y + targetHeight - globalPos.y,
                                vertices[verticeIndex].z);
                    continue;
                }

                //search the ground object for below the vertice
                RaycastHit[] hitsBelow = Physics.RaycastAll(vertices[verticeIndex], Vector3.down, float.PositiveInfinity);
                hit = FindGroundObj(hitsBelow);

                //if the ground object is found, go to the next vertice
                if (hit != null)
                {
                    vertices[verticeIndex] = new Vector3(
                                vertices[verticeIndex].x,
                                hit.Value.point.y + targetHeight - globalPos.y,
                                vertices[verticeIndex].z);
                    continue;
                }

                return false;  //failed to modify the vertice due to the lack of ground object on the top or bottom
            }

            mesh.vertices = vertices; //update vertices data to mesh

            return true;    //success to modify all vertices
        }


        /// <summary>
        /// Find the ground object from the RaycastHit array
        /// 
        /// If the ground object with cityObjectType set to COT_TINRelief is found, returns the RaycastHit.
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="hits"></param>
        /// <returns>RaycastHit of Ground obj</returns>
        RaycastHit? FindGroundObj(RaycastHit[] hits)
        {
            foreach (var hit in hits)
            {
                PLATEAUCityObjectGroup cityObjectData = hit.transform.GetComponent<PLATEAUCityObjectGroup>();
                if (cityObjectData == null) continue;

                foreach (var rootCityObject in cityObjectData.CityObjects.rootCityObjects)
                {
                    if (rootCityObject.CityObjectType == CityObjectType.COT_TINRelief)  //if the ground object is found
                    {
                        return hit;
                    }                       
                }
            }
            return null;
        }
    }
}
