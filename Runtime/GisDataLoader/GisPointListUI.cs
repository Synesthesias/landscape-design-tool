using Landscape2.Runtime.UiCommon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.GisDataLoader
{
    /// <summary>
    /// GISデータのポイントリストのプレゼンター
    /// </summary>
    public class GisPointListUI
    {
        private VisualElement rootPanel;
        private ScrollView scrollPanel;
        private VisualTreeAsset itemVisualTreeAsset;
        private GisPointInfos gisPointInfos;
        
        private readonly List<VisualElement> gisPointList = new();
        
        public GisPointListUI(GisPointInfos gisPointInfos, VisualElement root)
        {
            rootPanel = root.Q<VisualElement>("Panel_GisList");
            scrollPanel = rootPanel.Q<ScrollView>("Panel");
            
            this.gisPointInfos = gisPointInfos;
            
            // リストのアイテムのVisualTreeAssetをロード
            itemVisualTreeAsset = Resources.Load<VisualTreeAsset>("List_Gis");
            
            RegisterEvents();
            DeleteAll();
        }
        
        public void RegisterEvents()
        {
            gisPointInfos.OnDeleteAll.AddListener(DeleteAll);
            gisPointInfos.OnCreate.AddListener((int attributeIndex) =>
            {
                var info = gisPointInfos.GetByAttribute(attributeIndex);
                if (info != null)
                {
                    CreateItem(attributeIndex, info);
                }
            });
        }
        
        private string GetItemName(int attributeIndex) => "GIS_" + attributeIndex;

        private void CreateItem(int attributeIndex, GisPointInfo info)
        {
            var item = itemVisualTreeAsset.CloneTree();
            
            // IDを使って名前を決定
            item.name = GetItemName(attributeIndex);
            
            // 属性タイトルセット
            var gisName = item.Q<Label>("GisName");
            gisName.text = info.DisplayName;
            
            // ピンの色セット
            var pin = item.Q<VisualElement>("Icon_Pin");
            pin.style.unityBackgroundImageTintColor = new StyleColor(info.Color.ToColor());
            
            // 非表示ボタン
            var toggleButton = item.Q<Toggle>("Toggle_HideList");
            toggleButton.RegisterValueChangedCallback((evt) =>
            {
                // 地図上のピン表示切り替え
                gisPointInfos.SetShow(attributeIndex, !evt.newValue);
            });
            
            // 削除ボタン
            var deleteButton = item.Q<Button>("DeleteButton");
            deleteButton.RegisterCallback<ClickEvent>((evt) =>
            {
                Delete(attributeIndex);
            });
            
            gisPointList.Add(item);
            
            // Viewに追加
            scrollPanel.Add(item);
            
            // データがある場合はNoData非表示
            ShowNoData(false);
        }

        private void Show(int index, bool isShow)
        {
            var item = gisPointList.Find(i => i.name == GetItemName(index));
            item.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Delete(int ID)
        {
            // 確認ポップアップ表示
            ModalUI.ShowSelectModal("データ削除します", "本当によろしいですか？", ModalUI.SelectModalType.Info, null, () =>
            {
                // Indexからアイテムを削除
                var item = gisPointList.Find(i => i.name == GetItemName(ID));
                
                // データ削除
                gisPointInfos.Delete(ID);
                
                gisPointList.Remove(item);
                scrollPanel.Remove(item);
                
                // データがない場合はNoData表示
                if (gisPointList.Count == 0)
                {
                    ShowNoData(true);
                }
                
                // ポップアップ表示
                ModalUI.ShowModal("削除しました", "データを削除しました", true, false);
            });
        }

        private void DeleteAll()
        {
            foreach (var item in gisPointList)
            {
                scrollPanel.Remove(item);
            }
            gisPointList.Clear();
            
            ShowNoData(true);
        }
        
        private void ShowNoData(bool isShow)
        {
            var noDataLabel = rootPanel.Q<Label>("No_Data");
            noDataLabel.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}