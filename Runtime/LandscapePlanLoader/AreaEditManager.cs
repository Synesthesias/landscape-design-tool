using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 読み込んだ区画の編集を行うクラス
    /// </summary>
    public sealed class AreaEditManager
    {
        private AreaProperty editingAreaProperty;
        private AreaPropertyOrigin editingAreaPropertyOrigin;

        /// <summary>
        /// 区画の制限高さを変更し、オブジェクトに反映するメソッド
        /// </summary>
        /// <param name="newHeight">新規に設定する制限高さ</param>
        public void ChangeHeight(float newHeight)
        {
            editingAreaProperty.limitHeight = newHeight;
            
            editingAreaProperty.SetLocalPosition(
                new Vector3(
                editingAreaProperty.transform.localPosition.x,
                newHeight - editingAreaPropertyOrigin.limitHeight,
                editingAreaProperty.transform.localPosition.z
                ));

            float displayRate = Mathf.Clamp(newHeight / editingAreaProperty.wallMaxHeight, 0, 1);
            editingAreaProperty.wallMaterial.SetFloat("_DisplayRate", displayRate);
            editingAreaProperty.wallMaterial.SetFloat("_LineCount", newHeight / editingAreaProperty.lineOffset);
        }

        /// <summary>
        /// 編集の対象となる区画データを設定するメソッド
        /// </summary>
        /// <param name="targetAreaIndex">対象区画のAreaPropertyリストの要素番号</param>
        public void StartEdit(int targetAreaIndex)
        {
            editingAreaPropertyOrigin = AreasDataComponent.GetOriginProperty(targetAreaIndex);
            editingAreaProperty = AreasDataComponent.GetProperty(targetAreaIndex);
        }

        /// <summary>
        /// 編集対象を解除するメソッド
        /// </summary>
        public void StopEdit()
        {
            editingAreaProperty = null;
            editingAreaPropertyOrigin = null;
        }

        /// <summary>
        /// 区画の制限高さの最大値を取得するメソッド
        /// </summary>
        public float GetMaxHeight()
        {
            if (editingAreaProperty == null) return 0;

            float maxHeight = editingAreaProperty.wallMaxHeight;
            return maxHeight;
        }

        /// <summary>
        /// 現在の制限高さ値を取得するメソッド
        /// </summary>
        public float GetLimitHeight()
        {
            if (editingAreaProperty == null) return 0;

            float limitHeight = editingAreaProperty.limitHeight;
            return limitHeight;
        }

        /// <summary>
        /// 対象区画の全プロパティを読み込み時の値に初期化するメソッド
        /// </summary>
        /// <param name="targetAreaIndex">対象区画のリスト要素番号</param>
        public void ResetProperty(int targetAreaIndex)
        {
            AreasDataComponent.TryResetProperty(targetAreaIndex);
        }

    }
}
