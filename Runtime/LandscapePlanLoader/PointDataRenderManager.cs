using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;
using PlateauToolkit.Maps;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// The class to create Mesh Objects from the point data
    /// </summary>
    public sealed class PointDataRenderManager
    {
        List<GameObject> m_ListOfGISObjects = new List<GameObject>();
        CesiumGeoreference m_GeoRef;

        /// <summary>
        /// Create Mesh Objects from the given point data
        /// </summary>
        /// <param name="ParentObjectName"> Name of the parent object which has Mesh Objects </param>
        /// <param name="pointDatas"> List of Mesh vertex world point data </param>
        /// <param name="listOfGISObjects"> list which will have the created Mesh Objects </param>
        /// <returns></returns>
        public bool DrawShapes(string ParentObjectName, List<List<Vector3>> pointDatas, out List<GameObject> listOfGISObjects)
        {
            listOfGISObjects = null;
            if (pointDatas.Count > 0)
            {
                GameObject rootObject = new GameObject(ParentObjectName + "_GIS");
                m_GeoRef = GameObject.FindObjectOfType<CesiumGeoreference>();
                rootObject.transform.parent = m_GeoRef.transform;
                CesiumGlobeAnchor anchor = rootObject.AddComponent<CesiumGlobeAnchor>();
                rootObject.AddComponent<MeshFilter>();
                rootObject.AddComponent<MeshRenderer>();

                GameObject mesh = Resources.Load<GameObject>(PlateauToolkitMapsConstants.k_MeshObjectPrefab);

                DrawPolygonOrPolyline(rootObject, mesh, pointDatas);

                double3 pos = anchor.longitudeLatitudeHeight;
                pos.z = 0;
                anchor.longitudeLatitudeHeight = pos;

                listOfGISObjects = m_ListOfGISObjects;

                return true;
            }

            Debug.LogError("No point data included");
            return false;
        }

        void DrawPolygonOrPolyline(GameObject parentObject, GameObject originMeshObj, List<List<Vector3>> pointDatas)
        {
            foreach (List<Vector3> partPointsWorld in pointDatas)
            {
                if(partPointsWorld.Count < 3)
                {
                    Debug.LogError("Point data is empty");
                    continue;
                }

                GameObject meshObject = GameObject.Instantiate(originMeshObj);

                meshObject.transform.position = Vector3.zero;
                meshObject.transform.parent = parentObject.transform;

                // Create a tessellated mesh
                TessellatedMeshCreator tessellatedMeshCreator = new TessellatedMeshCreator();
                MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
                tessellatedMeshCreator.CreateTessellatedMesh(partPointsWorld, meshFilter, 30, 40);

                m_ListOfGISObjects.Add(meshObject);
            }
        }
    }
}
