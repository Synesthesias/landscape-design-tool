using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib.Buildings;
using System;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class ArrangementBuildingEditorSettingUI
    {
        private BuildingType buildingType;
        private VisualElement editPanel;
        
        public ArrangementBuildingEditorSettingUI(VisualElement element, BuildingType buildingTypeInstance)
        {
            buildingType = buildingTypeInstance;

            switch (buildingType)
            {
                case BuildingType.k_Apartment:
                    editPanel = element.Q<VisualElement>("Setting_Mansion");
                    break;
                case BuildingType.k_OfficeBuilding:
                    editPanel = element.Q<VisualElement>("Setting_Office");
                    break;
                case BuildingType.k_House:
                    editPanel = element.Q<VisualElement>("Setting_House");
                    break;
                case BuildingType.k_ConvenienceStore:
                    editPanel = element.Q<VisualElement>("Setting_Store");
                    break;
                case BuildingType.k_CommercialBuilding:
                case BuildingType.k_Hotel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            RegisterEditButtonAction();
            ShowPanel(false);
        }
        
        private void RegisterEditButtonAction()
        {
            // TODO: 個別パラメータは別途対応
        }
        
        private void ShowPanel(bool isShow)
        {
            editPanel.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}