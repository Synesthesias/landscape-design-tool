using UnityEngine;

namespace Landscape2.Runtime.Common
{
    public static class LayerMaskUtil
    {
        private static int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        private static int hiddenBuildingLayer = 13;
        public static string HiddenBuildingLayerName => "HiddenBuilding";
        
        public static void SetIgnore(GameObject target, bool isIgnore, int defaultLayer = 0)
        {
            if (defaultLayer == 0)
            {
                defaultLayer = LayerMask.NameToLayer("Default");
            }
            target.layer = isIgnore ? ignoreLayer : defaultLayer;
        }
        
        public static bool IsIgnore(GameObject target)
        {
            return target.layer == ignoreLayer;
        }
        
        public static void SetHiddenBuilding(GameObject target, bool isHidden, int defaultLayer = 0)
        {
            if (defaultLayer == 0)
            {
                defaultLayer = LayerMask.NameToLayer("Default");
            }
            target.layer = isHidden ? hiddenBuildingLayer : defaultLayer;
        }
        
        /// <summary>
        /// 地面クリック検出用のレイヤーマスクを取得
        /// Ignore RaycastとHiddenBuilding（非表示の建物）レイヤーを除外し、
        /// アセット配置や歩行者移動などの地面クリック操作で使用
        /// </summary>
        public static int GetGroundClickLayerMask()
        {
            int layerMask = LayerMask.GetMask("Ignore Raycast", HiddenBuildingLayerName);
            return ~layerMask;
        }
    }
}