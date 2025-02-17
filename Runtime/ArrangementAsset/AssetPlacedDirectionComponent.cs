using UnityEngine;

namespace Landscape2.Runtime
{
    public enum AssetPlacedDirectionType
    {
        InValid,
        ForwardToCamera, // 正面方向をカメラに向ける
    }

    /// <summary>
    /// アセット配置向き制御コンポーネント
    /// </summary>
    public class AssetPlacedDirectionComponent : MonoBehaviour
    {
        private AssetPlacedDirectionType type = AssetPlacedDirectionType.InValid;
        private bool isPlacing = true;
        
        private static AssetPlacedDirectionType TryCheckRequired(GameObject target)
        {
            // NOTE: 一旦全てのアセットでForwardToCameraにしているが、必要に応じて分岐を追加
            return AssetPlacedDirectionType.ForwardToCamera;
        }
        
        public static bool TryAdd(GameObject target)
        {
            var type = TryCheckRequired(target);
            if (type == AssetPlacedDirectionType.InValid)
            {
                return false;
            }
            
            var component = target.GetComponent<AssetPlacedDirectionComponent>();
            if (component == null)
            {
                component = target.AddComponent<AssetPlacedDirectionComponent>();
            }
            component.SetType(type);
            return true;
        }
        
        public void SetPlaced()
        {
            isPlacing = false;
        }
        
        public void SetType(AssetPlacedDirectionType type)
        {
            this.type = type;
        }

        private void Update()
        {
            if (!isPlacing)
            {
                // 配置が完了したら何もしない
                return;
            }

            if (type == AssetPlacedDirectionType.ForwardToCamera)
            {
                TrySetForward();
            }
        }

        private void TrySetForward()
        {
            // カメラの方向に対して正面か裏かを判定
            Vector3 dirToCamera = Camera.main.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, dirToCamera.normalized);
            if (dot < 0)
            {
                // 裏の場合は、Y軸を中心に180度回転させて正面にする。
                transform.Rotate(0, 180, 0);
            }
        }
    }
}