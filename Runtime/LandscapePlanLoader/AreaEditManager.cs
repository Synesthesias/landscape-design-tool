using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    public class AreaEditManager
    {
        private AreaProperty editingAreaProperty;
        private AreaPropertyOrigin editingAreaPropertyOrigin;

        public AreaEditManager()
        {
        }

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

        public void StartEdit(int targetAreaIndex)
        {
            editingAreaPropertyOrigin = AreasDataComponent.GetOriginProperty(targetAreaIndex);
            editingAreaProperty = AreasDataComponent.GetProperty(targetAreaIndex);
        }

        public float GetMaxHeight()
        {
            if (editingAreaProperty == null) return 0;

            float maxHeight = editingAreaProperty.wallMaxHeight;
            return maxHeight;
        }

        public void StopEdit()
        {
            editingAreaProperty = null;
            editingAreaPropertyOrigin = null;
        }

        public float GetLimitHeight()
        {
            if (editingAreaProperty == null) return 0;

            float limitHeight = editingAreaProperty.limitHeight;
            return limitHeight;
        }

        public void ResetProperty(int targetAreaIndex)
        {
            AreasDataComponent.TryResetProperty(targetAreaIndex);
        }

    }
}
