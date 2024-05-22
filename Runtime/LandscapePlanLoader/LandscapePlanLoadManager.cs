using System.Collections.Generic;
using UnityEngine;
using SFB;
using PlateauToolkit.Maps;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// The class to load GIS data and generate walls
    /// </summary>
    public sealed class LandscapePlanLoadManager
    {
        List<GameObject> listOfGISObjects;

        /// <summary>
        /// Load GIS data from the given folder path
        /// </summary>
        /// <param name="gisTargetFolderPath"> The path to the folder containing .shp and .dbf files </param>
        public void LoadShapefile(string gisTargetFolderPath)
        {
            // Load GIS data and create Mesh objects
            using (ShapefileRenderManager m_ShapefileRenderManager = new ShapefileRenderManager(gisTargetFolderPath, 0 /*RenderMode:Mesh*/, 0, false, false, SupportedEncoding.ShiftJIS, null))
            {
                if (m_ShapefileRenderManager.Read(0, out listOfGISObjects))
                {
                    Debug.Log("Loading GIS data completed");
                }
                else
                {
                    Debug.LogError("Loading GIS data failed.");
                    return;
                }
            }

            if (listOfGISObjects == null || listOfGISObjects.Count == 0)
            {
                Debug.LogError("No GIS data loaded");
                return;
            }

            // Modify the mesh objects to the target height and generate walls
            LandscapePlanMeshModifier landscapePlanMeshModifier = new LandscapePlanMeshModifier();
            WallGenerator wallGenerator = new WallGenerator();

            foreach (GameObject gisObject in listOfGISObjects)
            {
                MeshFilter meshFilter = gisObject.GetComponent<MeshFilter>();
                DbfComponent dbf = gisObject.GetComponent<DbfComponent>();

                if (meshFilter == null)
                {
                    Debug.LogError("This GameObject have no MeshFilter Component" + "\n" + $"ObjectName:{gisObject.name}");
                    return;
                }
                if (meshFilter.sharedMesh == null)
                {
                    Debug.LogError("Mesh in MeshFilter is null" + "\n" + $"ObjectName:{gisObject.name}");
                    return;
                }
                if (dbf == null)
                {
                    Debug.LogError("This GameObject have no DbfComponent" + "\n" + $"ObjectName:{gisObject.name}");
                    return;
                }

                float heightLimit = float.Parse(dbf.Properties[3]); //read height data from dbf file
                Mesh mesh = meshFilter.sharedMesh;  //read mesh data from meshFilter

                //modify mesh data
                if (!landscapePlanMeshModifier.TryModifyMeshToTargetHeight(mesh, heightLimit, gisObject.transform.position))
                    Debug.LogError("This area is out of range of the loaded map" + "\n" + $"ObjectName:{gisObject.name}");

                // Generate a wall downward from the mesh
                GameObject wall = wallGenerator.GenerateWall(mesh, heightLimit, Vector3.down); 
                wall.transform.SetParent(gisObject.transform);
                wall.transform.localPosition = Vector3.zero;
            }
            Debug.Log("Mesh modification and wall generation completed");
        }

        /// <summary>
        /// Poopup a dialog to select a folder and get the path
        /// </summary>
        /// <returns></returns>
        public string BrowseFolder()
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Open Folder", "", false);
            
            if (paths.Length > 0) return paths[0];
            return null;
        }
    }
}

