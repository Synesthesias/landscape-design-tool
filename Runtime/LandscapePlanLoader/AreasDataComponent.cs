using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// The class holds all data related to the area and manages editing.
    /// </summary>
    public sealed class AreasDataComponent : ISubComponent
    {
        // The property of the area. This data can be edited.
        private static List<AreaProperty> properties;

        // The origin property of the area. This data changes only when new area is loaded.
        private static List<AreaPropertyOrigin> propertiesOrigin;

        public AreasDataComponent()
        {
            properties = new List<AreaProperty>();
            propertiesOrigin = new List<AreaPropertyOrigin>();
        }

        /// <summary>
        /// Add new area property to database.
        /// </summary>
        /// <param name="newProperty"></param>
        public static void AddNewProperty(AreaProperty newProperty)
        {
            AreaPropertyOrigin newPropertyOrigin = new AreaPropertyOrigin(
                newProperty.ID, 
                newProperty.name, 
                newProperty.limitHeight, 
                newProperty.lineOffset, 
                newProperty.color, 
                newProperty.referencePosition, 
                newProperty.transform.position
                );

            properties.Add(newProperty);
            propertiesOrigin.Add(newPropertyOrigin);
        }

        /// <summary>
        /// Reset the property to the origin data.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TryResetProperty(int index)
        {
            if (index < 0 && index >= properties.Count) return false;

            properties[index].ID = propertiesOrigin[index].ID;
            properties[index].name = propertiesOrigin[index].name;
            properties[index].limitHeight = propertiesOrigin[index].limitHeight;
            properties[index].lineOffset = propertiesOrigin[index].lineOffset;
            properties[index].color = propertiesOrigin[index].color;
            properties[index].SetLocalPosition(Vector3.zero);

            properties[index].ceilingMaterial.color = properties[index].color;
            properties[index].wallMaterial.color = properties[index].color;
            properties[index].wallMaterial.SetFloat("_DisplayRate", properties[index].limitHeight / properties[index].wallMaxHeight);
            properties[index].wallMaterial.SetFloat("_LineCount", properties[index].limitHeight / properties[index].lineOffset);

            return true;
        }

        /// <summary>
        /// Get the property
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static AreaProperty GetProperty(int index)
        {
            if (index < 0 && index >= properties.Count) return null;
            return properties[index];
        }

        /// <summary>
        /// Get origin property
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static AreaPropertyOrigin GetOriginProperty(int index)
        {
            if (index < 0 && index >= propertiesOrigin.Count) return null;
            return propertiesOrigin[index];
        }

        /// <summary>
        /// Delete the property
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TryRemoveProperty(int index)
        {
            if (index < 0 && index >= properties.Count) return false;

            properties.RemoveAt(index);
            propertiesOrigin.RemoveAt(index);
            return true;
        }
        
        /// <summary>
        /// Get the count of all properties
        /// </summary>
        /// <returns></returns>
        public static int GetPropertyCount()
        {
            return properties.Count;
        }


        public void OnDisable()
        {
        }
        public void OnEnable()
        {
        }
        public void Start()
        {
        }
        public void Update(float deltaTime)
        {
        }
    }
    
    /// <summary>
    /// Template class of editable area data
    /// </summary>
    public class AreaProperty
    {
        public int ID { get; set;}
        public string name { get; set;}
        public float limitHeight { get; set;}
        public float lineOffset { get; set;}
        public Color color { get; set;}
        public Material wallMaterial { get; private set;}
        public Material ceilingMaterial { get; private set;}

        public float wallMaxHeight { get; private set; }
        public Transform transform { get; private set; }
        public Vector3 referencePosition { get; private set; }
        public List<Vector3> pointData { get; private set; }

        public AreaProperty(int ID, string name, float limitHeight, float lineOffset, Color areaColor, Material wallMaterial, Material ceilingMaterial, float wallMaxHeight, Vector3 referencePos, Transform areaTransform, List<Vector3> pointData)
        {
            this.ID = ID;
            this.name = name;
            this.limitHeight = limitHeight;
            this.lineOffset = lineOffset;
            this.color = areaColor;
            this.wallMaterial = wallMaterial;
            this.ceilingMaterial = ceilingMaterial;
            this.wallMaxHeight = wallMaxHeight;
            this.referencePosition = referencePos;
            this.transform = areaTransform;
            this.pointData = pointData;
        }

        public void SetLocalPosition(Vector3 newPosition)
        {
            referencePosition += newPosition - transform.position;
            transform.localPosition = newPosition;
        }
    }

    /// <summary>
    /// Template class of origin area data
    /// </summary>
    public class AreaPropertyOrigin
    {
        public int ID { get; private set; }
        public string name { get; private set; }
        public float limitHeight { get; private set; }
        public float lineOffset { get; private set; }
        public Color color { get; private set; }
        public Vector3 position { get; private set; }
        public Vector3 referencePosition { get; private set; }

        public AreaPropertyOrigin(int ID, string name,float limitHeight, float lineOffset, Color areaColor, Vector3 referencePos, Vector3 areaPosition)
        {
            this.ID = ID;
            this.name = name;
            this.limitHeight = limitHeight;
            this.lineOffset = lineOffset;
            this.color = new Color(areaColor.r, areaColor.g, areaColor.b, areaColor.a);
            this.referencePosition = referencePos;
            this.position = areaPosition;
        }
    }
}
