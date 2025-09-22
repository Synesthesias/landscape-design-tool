using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.Common
{
    /// <summary>
    /// UI関連のユーティリティクラス
    /// </summary>
    public static class UIUtils
    {
        // このClassNameはUnityのバージョンにより変わる可能性があるので注意
        private const string DropDownPanelClassName = "unity-base-dropdown__container-inner";

        /// <summary>
        /// ドロップダウンフィールドがあるUIのセットアップ
        /// ドロップダウンフィールドのポップアップ上はレイヤーが新設されるため、
        /// 下のフィールドのPickingModeを無視してシーン上にイベントを伝播してしまうのでそれを封じる関数
        /// </summary>
        /// <param name="onDisableInputEvent">ドロップダウンフィールドのポップアップ展開時にシーン上のイベントを無効化する関数</param>
        /// <param name="onEnableInputEvent">ドロップダウンフィールドのポップアップ終了時にシーン上のイベントを有効化する関数</param>
        public static void DropdownFieldSetup(VisualElement uiRoot, Action onDisableInputEvent, Action onEnableInputEvent)
        {
            // VisualElementのドロップダウン類はクリックイベントをUIでブロック出来ずシーンへ伝えてしまう挙動の仕様がある為、
            // ドロップダウンを開いた際にシーン側の入力を無効化し、閉じたときに有効化する対策を入れる
            List<DropdownField> allDropdowns = new List<DropdownField>(uiRoot.Query<DropdownField>().ToList());

            foreach (DropdownField dropdown in allDropdowns)
            {
                dropdown.RegisterCallback<PointerDownEvent>(dropdownEvent =>
                {
                    onDisableInputEvent?.Invoke();

                    // 開いた直後は Popup がまだ生成されていないので遅延実行する
                    dropdown.schedule.Execute(() =>
                    {
                        // ドロップダウンを閉じたときのイベントが現バージョンには存在しない為、特定のVisualElementがツリーから消えたときを検知して対応する...
                        var popup = dropdown.panel.visualTree.Q<VisualElement>(className: DropDownPanelClassName);
                        if (popup == null)
                        {
                            return;
                        }

                        popup.RegisterCallback<DetachFromPanelEvent>(popupEvent =>
                        {
                            onEnableInputEvent?.Invoke();
                        });
                    });
                });
            }
        }
    }
}