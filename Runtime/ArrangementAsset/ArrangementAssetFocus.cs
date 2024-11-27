using PlateauToolkit.Sandbox.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using PlateauSandboxBuilding = PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime.PlateauSandboxBuilding;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 配置したアセットにフォーカスするクラス
    /// </summary>
    public class ArrangementAssetFocus
    {
        private LandscapeCamera landscapeCamera;
        
        private Color focusColor = new Color32(255,195,195,255);
        private const float focusDuration = 1.0f;
        private bool isFocusing = false;
        
        public ArrangementAssetFocus(LandscapeCamera landscapeCamera)
        {
            this.landscapeCamera = landscapeCamera;
        }

        public void Focus(GameObject target)
        {
            if (isFocusing)
            {
                return;
            }
            
            if (landscapeCamera.cameraState != LandscapeCameraState.PointOfView)
            {
                return;
            }
            isFocusing = true;
            landscapeCamera.FocusPoint(target, () =>
            {
                // 完了後アセットのカラー変更
                SetEmissive(target);
            });
        }
        
        private void SetEmissive(GameObject target)
        {
            var materials = GetMaterials(target);
            foreach (var material in materials)
            {
                SetMaterialEmissiveAsync(material);
            }
        }
        
        private List<Material> GetMaterials(GameObject target)
        {
            List<Material> materials = new List<Material>();
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    materials.Add(material);
                }
            }
            return materials;
        }

        private async void SetMaterialEmissiveAsync(Material material)
        {
            var initColor = material.GetColor("_BaseColor");
            material.SetColor("_BaseColor", focusColor);
            
            await Task.Delay((int)(focusDuration * 1000));
            
            // 元に戻す
            material.SetColor("_BaseColor", initColor);
            
            isFocusing = false;
        }
    }
}