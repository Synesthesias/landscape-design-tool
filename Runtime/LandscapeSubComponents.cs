using System.Collections.Generic;
using Landscape2.Runtime.CameraPositionMemory;
using Landscape2.Runtime.UiCommon;
using Landscape2.Runtime.WeatherTimeEditor;
using Landscape2.Runtime.LandscapePlanLoader;
using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 実行時の機能である<see cref="ISubComponent"/>をここにまとめて、UpdateやOnEnable等を呼び出します。
    /// </summary>
    
    public enum SubMenuUxmlType
    {
        Menu = -1,
        EditBuilding,
        Asset,
        Bim,
        Gis,
        Planning,
        Analytics,
        CameraList,
        CameraEdit,
    }

    public class LandscapeSubComponents : MonoBehaviour
    {
        private List<ISubComponent> subComponents;
        // 現在開かれているサブメニュー機能
        private SubMenuUxmlType subMenuUxmlType = SubMenuUxmlType.Menu;
        // サブメニューのuxmlを管理するする配列
        VisualElement[] subMenuUxmls;

        private void Awake()
        {
            var mainCam = Camera.main;
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("GlobalNavi_Main");

            // GlobalNavi_Main.uxmlのSortOrderを設定
            GameObject.Find("GlobalNavi_Main").GetComponent<UIDocument>().sortingOrder = 1;

            // サブメニューのuxmlを生成して非表示
            subMenuUxmls = new VisualElement[Enum.GetNames(typeof(SubMenuUxmlType)).Length - 1];
            for (int i = 0; i < subMenuUxmls.Length; i++)
            {
                subMenuUxmls[i] = new UIDocumentFactory().CreateWithUxmlName(((SubMenuUxmlType)i).ToString());
                subMenuUxmls[i].style.display = DisplayStyle.None;
            }

            // 必要な機能をここに追加します
            // ※GlobalNaviと各機能の紐づけ作業が完了するまで一部機能はコメントアウトしています
            subComponents = new List<ISubComponent>
            {
                //new CameraMoveByUserInput(mainCam),
                //new CameraPositionMemoryUI(new CameraPositionMemory.CameraPositionMemory(mainCam)),
                //new AreasDataComponent(),
                //new LandscapePlanLoaderUI(),
                //new LandscapePlanEditorUI(),
                //new ArrangeAsset(),
                // RegulationAreaUI.CreateForScene(),
                //LineOfSightUI.CreateForScene(),
                new GlobalNaviHeader(uiRoot,subMenuUxmls),
                new WeatherTimeEditorUI(new WeatherTimeEditor.WeatherTimeEditor(),uiRoot),
                //new SaveSystem()
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

        public SubMenuUxmlType GetSubMenuUxmlType()
        {
            return subMenuUxmlType;
        }

        public void SetSubMenuUxmlType(SubMenuUxmlType type)
        {
            subMenuUxmlType = type;
        }
    }
}
