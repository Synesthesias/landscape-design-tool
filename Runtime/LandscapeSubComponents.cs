using System.Collections.Generic;
using Landscape2.Runtime.CameraPositionMemory;
using Landscape2.Runtime.UiCommon;
using Landscape2.Runtime.WeatherTimeEditor;
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
            var mainCam = Camera.main;
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("GlobalNavi_Main");

            // 必要な機能をここに追加します
            subComponents = new List<ISubComponent>
            {
                new CameraMoveByUserInput(mainCam),
                new CameraPositionMemoryUI(new CameraPositionMemory.CameraPositionMemory(mainCam)),
                new ArrangeAsset(),
                RegulationAreaUI.CreateForScene(),
                LineOfSightUI.CreateForScene(),
                new WeatherTimeEditorUI(new WeatherTimeEditor.WeatherTimeEditor(),uiRoot),
            };
        }

        private void Start()
        {
            foreach(var c in subComponents)
            {
                c.Start();
            }
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
