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
        Material wallMaterial;
        Material ceilingMaterial;

        public LandscapePlanLoadManager()
        {
            wallMaterial = Resources.Load<Material>("Materials/PlanAreaWallMaterial");
            ceilingMaterial = Resources.Load<Material>("Materials/PlanAreaCeilingMaterial");
            ceilingMaterial.SetFloat("_Alpha", 0.2f);
            ceilingMaterial.SetFloat("_Intensity", 3f);
        }

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
                // Get gis property datas from dbf file
                DbfComponent dbf = gisObject.GetComponent<DbfComponent>();
                if (dbf == null)
                {
                    Debug.LogError("GisObject have no DbfComponent");
                    return;
                }
                float dbfID = float.Parse(dbf.Properties[0]);
                float heightLimit = float.Parse(dbf.Properties[3]);
                Color AreaColor = DbfStringToColor(dbf.Properties[4]);
                
                gisObject.name = $"Area_{dbfID}";


                MeshFilter gisObjMeshFilter = gisObject.GetComponent<MeshFilter>();
                MeshRenderer gisObjMeshRenderer = gisObject.GetComponent<MeshRenderer>();
                if (gisObjMeshFilter == null)
                {
                    Debug.LogError($"{gisObject.name} have no MeshFilter Component");
                    return;
                }
                if (gisObjMeshRenderer == null)
                {
                    Debug.LogError($"{gisObject.name} have no MeshRenderer Component");
                    return;
                }

                // Set material to the ceiling
                Material ceilingMatInstance = new Material(ceilingMaterial);
                ceilingMatInstance.color = AreaColor;
                gisObjMeshRenderer.material = ceilingMatInstance;

                // Modify mesh data
                Mesh mesh = gisObjMeshFilter.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogError($"Mesh in MeshFilter of {gisObject.name} is null");
                    return;
                }
                if (!landscapePlanMeshModifier.TryModifyMeshToTargetHeight(mesh, heightLimit, gisObject.transform.position))
                {
                    Debug.LogError($"{gisObject.name} is out of range of the loaded map");
                    return;
                }

                // Generate a wall downward from the mesh
                Material wallMatInstance = new Material(wallMaterial);
                wallMatInstance.color = AreaColor;
                GameObject wall = wallGenerator.GenerateWall(mesh, heightLimit, Vector3.down, wallMatInstance); 
                wall.transform.SetParent(gisObject.transform);
                wall.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                wall.name = $"AreaWall_{dbfID}";
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

        /// <summary>
        /// Convert the color string from the dbf file to Color
        /// </summary>
        /// <param name="colorString"> The color string in the format of "r,g,b,a" </param>
        /// <returns></returns>
        Color DbfStringToColor(string colorString)
        {
            string[] colorValues = colorString.Split(',');

            if (colorValues.Length == 4 &&
                float.TryParse(colorValues[0], out float r) &&
                float.TryParse(colorValues[1], out float g) &&
                float.TryParse(colorValues[2], out float b) &&
                float.TryParse(colorValues[3], out float a))
            {
                return new Color(r, g, b, a);
            }

            Debug.LogError("Invalid color string format.");
            return Color.clear;
        }
    }
}

