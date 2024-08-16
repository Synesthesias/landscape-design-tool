using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using Landscape2.Runtime.UiCommon;
using System;

namespace Landscape2.Runtime
{
    /// <summary>
    /// GlobalNaviのヘッダー処理
    /// </summary>
    public class GlobalNaviHeader : ISubComponent
    {
        // メニューボタングループ
        private readonly GroupBox menuGroup;
        // サブメニューパネル
        private readonly VisualElement subMenuElement;
        // 設定ボタン
        private readonly Button settingButton;
        // 設定パネル
        private readonly VisualElement settingElement;
        // メニューボタングループ名前
        private const string UImenuGroup = "Menu";
        // サブメニューパネル名前
        private const string UISubMenuElement = "SubMenu";
        // 設定ボタン名前
        private const string UISettingButton = "Button_Setting";
        // 設定パネル名前
        private const string UISettingElement = "SettingPanel";
        // 最後に開いたサブメニューuxml
        private VisualElement lastSubMenuUxml;

        public GlobalNaviHeader(VisualElement uiRoot, VisualElement[] subMenuUxmls)
        {
            menuGroup = uiRoot.Q<GroupBox>(UImenuGroup);
            subMenuElement = uiRoot.Q<VisualElement>(UISubMenuElement);
            settingButton = uiRoot.Q<Button>(UISettingButton);
            settingElement = uiRoot.Q<VisualElement>(UISettingElement);

            // サブメニューを非表示
            foreach (var child in subMenuElement.Children())
            {
                child.style.display = DisplayStyle.None;
            }

            // 設定パネルを非表示
            settingElement.style.display = DisplayStyle.None;

            // メニューボタンリスト
            var menuButtons = menuGroup.Children();
            // メニューボタンの値が変更されたとき
            menuButtons.ToList().ForEach(r =>
            {
                var button = r as RadioButton;
                button.RegisterValueChangedCallback(evt =>
                {
                    if (button.value)
                    {
                        // tabIndexに該当するサブメニューを表示
                        ShowSubMenuPanel(button.tabIndex);
                    }
                });
            });

            // サブメニューボタンの値が変更されたとき
            subMenuElement.Children().ToList().ForEach(g =>
            {
                var groupBox = g as GroupBox;
                groupBox.Children().ToList().ForEach(r =>
                {
                    var button = r as RadioButton;
                    button.RegisterValueChangedCallback(evt =>
                    {
                        if (button.value)
                        {
                            // tabIndexに該当するuxmlを表示
                            subMenuUxmls[button.tabIndex].style.display = DisplayStyle.Flex;
                            SetSubMenuUxml(button.tabIndex);
                            lastSubMenuUxml = subMenuUxmls[button.tabIndex];
                        }
                    });
                });
            });

            // 設定ボタンがクリックされたとき
            settingButton.clicked += () =>
            {
                // 設定パネルの表示/非表示を切り替え
                if (settingElement.style.display == DisplayStyle.Flex)
                {
                    settingElement.style.display = DisplayStyle.None;
                }
                else
                {
                    settingElement.style.display = DisplayStyle.Flex;
                }
            };
        }

        // サブメニューを選択するパネルを表示する
        private void ShowSubMenuPanel(int id)
        {
            // 最後に開いたサブメニューのuxmlを非表示
            if (lastSubMenuUxml != null)
            {
                lastSubMenuUxml.style.display = DisplayStyle.None;
            }

            foreach (var subMenu in subMenuElement.Children())
            {
                // 全てのサブメニューを非表示
                subMenu.style.display = DisplayStyle.None;

                // サブメニューのラジオボタンを全てfalseにする
                foreach (var subMenuButton in subMenu.Children())
                {
                    var button = subMenuButton as RadioButton;
                    button.value = false;
                }
            }

            // 指定のサブメニューを表示
            if (id >= 0) // tabIndexが-1(Menu)の場合は何も表示しない
            {
                subMenuElement.Children().ElementAt(id).style.display = DisplayStyle.Flex;
            }
        }

        // サブメニューのuxmlを表示する
        private void SetSubMenuUxml(int id)
        {
            // 最後に開いたサブメニューのuxmlを非表示
            if (lastSubMenuUxml != null)
            {
                lastSubMenuUxml.style.display = DisplayStyle.None;
            }

            // 現在開かれているサブメニューのuxmlを設定
            var type = (SubMenuUxmlType)Enum.ToObject(typeof(SubMenuUxmlType), id);
            var subComponents = GameObject.FindObjectOfType<LandscapeSubComponents>();
            if (subComponents != null)
            {
                subComponents.SetSubMenuUxmlType(type);
            }
        }
        public void Update(float deltaTime)
        {
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
        }
        public void Start()
        {
        }
    }
}
