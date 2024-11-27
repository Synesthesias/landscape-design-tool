using Landscape2.Runtime.UiCommon;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 配置したアセットの一覧のアイテム
    /// </summary>
    public class ArrangementAssetListItemUI
    {
        public ArrangementAssetListItem Model { get; private set; }
        
        public UnityEvent OnDeleteAsset = new();
        public UnityEvent OnFocusAsset = new();
        
        public ArrangementAssetListItemUI(
            int prefabID,
            VisualElement element,
            ArrangementAssetType type,
            int typeIndex)
        {
            var assetName = element.Q<Label>("AssetName");
            
            // カテゴリー名 + インデックスで名前を表現
            assetName.text = type.GetCategoryName() + "_" + typeIndex;
            
            Model = new ArrangementAssetListItem(prefabID, element, type);
            
            RegisterButtons();
        }

        private void RegisterButtons()
        {
            var deleteButton = Model.Element.Q<Button>("DeleteButton");
            deleteButton.clicked += () =>
            {
                OnDeleteAsset.Invoke();
            };
            
            var focusButton = Model.Element.Q<Button>("List");
            focusButton.clicked += () =>
            {
                OnFocusAsset.Invoke();
            };
        }
        
    }
}