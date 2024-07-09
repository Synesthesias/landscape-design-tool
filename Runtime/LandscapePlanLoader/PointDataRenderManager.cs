using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;
using PlateauToolkit.Maps;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    public class PointDataRenderManager
    {
        List<GameObject> m_ListOfGISObjects = new List<GameObject>();
        CesiumGeoreference m_GeoRef;

        public bool DrawShapes(string currentRenderingObjectName, List<List<Vector3>> pointDatas, out List<GameObject> listOfGISObjects)
        {
            listOfGISObjects = null;
            if (pointDatas.Count > 0)
            {
                GameObject rootShpObject = new GameObject(currentRenderingObjectName + "_SHP");
                rootShpObject.transform.parent = m_GeoRef.transform;
                CesiumGlobeAnchor anchor = rootShpObject.AddComponent<CesiumGlobeAnchor>();
                rootShpObject.AddComponent<MeshFilter>();
                rootShpObject.AddComponent<MeshRenderer>();

                GameObject mesh = Resources.Load<GameObject>(PlateauToolkitMapsConstants.k_MeshObjectPrefab);

                DrawPolygonOrPolyline(rootShpObject, mesh, pointDatas);

                double3 pos = anchor.longitudeLatitudeHeight;
                pos.z = 0;
                anchor.longitudeLatitudeHeight = pos;

                listOfGISObjects = m_ListOfGISObjects;

                return true;
            }
            return false;
        }

        void DrawPolygonOrPolyline(GameObject parentObject, GameObject mesh, List<List<Vector3>> pointDatas)
        {
            foreach (List<Vector3> partPointsWorld in pointDatas)
            {
                GameObject meshObject = GameObject.Instantiate(mesh);

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
