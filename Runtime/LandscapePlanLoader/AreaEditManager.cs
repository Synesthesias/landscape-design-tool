using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// The class tha manages the editing of the area and holds all data related to the area.
    /// </summary>
    public sealed class AreaEditManager
    {
        private AreaProperty editingAreaProperty;
        private AreaPropertyOrigin editingAreaPropertyOrigin;

        /// <summary>
        /// Change the limit height data of the area and reflect it to the object.
        /// </summary>
        /// <param name="newHeight"></param>
        public void ChangeHeight(float newHeight)
        {
            editingAreaProperty.limitHeight = newHeight;
            
            editingAreaProperty.SetLocalPosition(
                new Vector3(
                editingAreaProperty.transform.localPosition.x,
                newHeight - editingAreaPropertyOrigin.limitHeight,
                editingAreaProperty.transform.localPosition.z
                ));

            float displayRate = Mathf.Clamp(newHeight / editingAreaProperty.wallMaxHeight, 0, 1);
            editingAreaProperty.wallMaterial.SetFloat("_DisplayRate", displayRate);
            editingAreaProperty.wallMaterial.SetFloat("_LineCount", newHeight / editingAreaProperty.lineOffset);
        }

        /// <summary>
        /// Set target area data to edit
        /// </summary>
        /// <param name="targetAreaIndex"></param>
        public void StartEdit(int targetAreaIndex)
        {
            editingAreaPropertyOrigin = AreasDataComponent.GetOriginProperty(targetAreaIndex);
            editingAreaProperty = AreasDataComponent.GetProperty(targetAreaIndex);
        }

        /// <summary>
        /// Unset target area data
        /// </summary>
        public void StopEdit()
        {
            editingAreaProperty = null;
            editingAreaPropertyOrigin = null;
        }

        /// <summary>
        /// Get the max limit height of the area.
        /// </summary>
        /// <returns></returns>
        public float GetMaxHeight()
        {
            if (editingAreaProperty == null) return 0;

            float maxHeight = editingAreaProperty.wallMaxHeight;
            return maxHeight;
        }

        /// <summary>
        /// Get the current limit height of the area.
        /// </summary>
        /// <returns></returns>
        public float GetLimitHeight()
        {
            if (editingAreaProperty == null) return 0;

            float limitHeight = editingAreaProperty.limitHeight;
            return limitHeight;
        }

        /// <summary>
        /// Reset the property of the area to the origin property.
        /// </summary>
        /// <param name="targetAreaIndex"></param>
        public void ResetProperty(int targetAreaIndex)
        {
            AreasDataComponent.TryResetProperty(targetAreaIndex);
        }

    }
}
