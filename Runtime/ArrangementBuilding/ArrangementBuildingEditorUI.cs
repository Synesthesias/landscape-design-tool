using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Runtime;
using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 配置ビルのUIを提供するクラス
    /// </summary>
    public class ArrangementBuildingEditorUI
    {
        private VisualElement editPanel;
        private ArrangementBuildingEditor arrangementBuildingEditor = new();
        
        private VisualElement heightContainer;
        private SliderInt heightSlider;
        private SliderInt widthSlider;
        private SliderInt depthSlider;
        
        public ArrangementBuildingEditorUI(VisualElement element)
        {
            editPanel = element.Q<VisualElement>("EditBuildingArea");
            RegisterEditButtonAction();
            
            // 個別パラメータ
            var apartment = new ArrangementBuildingEditorSettingUI(editPanel, BuildingType.k_Apartment);
            var officeBuilding = new ArrangementBuildingEditorSettingUI(editPanel, BuildingType.k_OfficeBuilding);
            var house = new ArrangementBuildingEditorSettingUI(editPanel, BuildingType.k_House);
            var convenienceStore = new ArrangementBuildingEditorSettingUI(editPanel, BuildingType.k_ConvenienceStore);

            // デフォルト非表示
            ShowPanel(false);
        }
        
        private void RegisterEditButtonAction()
        {
            heightContainer = editPanel.Q<VisualElement>("HeightContainer");
            heightSlider = editPanel.Q<SliderInt>("HeightSlider");
            heightSlider.RegisterValueChangedCallback((evt) => OnHeightSliderChanged(evt.newValue));
            
            widthSlider = editPanel.Q<SliderInt>("WidthSlider");
            widthSlider.RegisterValueChangedCallback((evt) => OnWidthSliderChanged(evt.newValue));
            
            depthSlider = editPanel.Q<SliderInt>("DepthSlider");
            depthSlider.RegisterValueChangedCallback((evt) => OnDepthSliderChanged(evt.newValue));
        }
        
        public void ShowPanel(bool isShow)
        {
            editPanel.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        private void OnHeightSliderChanged(float value)
        {
            arrangementBuildingEditor.SetHeight(value);
        }
        
        private void OnWidthSliderChanged(float value)
        {
            arrangementBuildingEditor.SetWidth(value);
        }
        
        private void OnDepthSliderChanged(float value)
        {
            arrangementBuildingEditor.SetDepth(value);
        }

        public void TryShowPanel(GameObject selectObject)
        {
            if (selectObject.TryGetComponent<PlateauSandboxBuilding>(out var building))
            {
                arrangementBuildingEditor.SetTarget(building);
                InitializeSliders();
                ShowPanel(true);
                return;
            }
            ShowPanel(false);
        }
        
        private void InitializeSliders()
        {
            if (arrangementBuildingEditor.CanSlideHeight())
            {
                heightContainer.style.display = DisplayStyle.Flex;
                heightSlider.value = (int)arrangementBuildingEditor.GetHeight();
                heightSlider.lowValue = (int)arrangementBuildingEditor.GetMinAndMaxHeight().min;
                heightSlider.highValue = (int)arrangementBuildingEditor.GetMinAndMaxHeight().high;
            }
            else
            {
                heightContainer.style.display = DisplayStyle.None;
            }

            widthSlider.value = (int)arrangementBuildingEditor.GetWidth();
            widthSlider.lowValue = (int)arrangementBuildingEditor.GetMinAndMaxWidth().min;
            widthSlider.highValue = (int)arrangementBuildingEditor.GetMinAndMaxWidth().high;
            
            depthSlider.value =  (int)arrangementBuildingEditor.GetDepth();
            depthSlider.lowValue = (int)arrangementBuildingEditor.GetMinAndMaxDepth().min;
            depthSlider.highValue = (int)arrangementBuildingEditor.GetMinAndMaxDepth().high;
        }
    }
}