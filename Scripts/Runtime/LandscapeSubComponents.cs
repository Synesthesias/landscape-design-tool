using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 実行時の機能である<see cref="ISubComponent"/>をここにまとめて、UpdateやOnEnable等を呼び出します。
    /// </summary>
    public class LandscapeSubComponents : MonoBehaviour
    {
        private List<ISubComponent> subComponents;

        private void Awake()
        {
            // 必要な機能をここに追加します
            subComponents = new List<ISubComponent>
            {
                new CameraMoveByUserInput(Camera.main)
            };
        }
        
        private void OnEnable()
        {
            foreach (var c in subComponents)
            {
                c.OnEnable();
            }
        }

        private void Update()
        {
            foreach (var c in subComponents)
            {
                c.Update(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            foreach (var c in subComponents)
            {
                c.OnDisable();
            }
        }
    }
}
