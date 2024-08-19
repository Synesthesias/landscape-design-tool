using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 読み込んだ区画の編集を行うクラス
    /// </summary>
    public sealed class AreaEditManager
    {
        private AreaProperty editingAreaProperty;
        private AreaPropertySnapshot editingAreaPropertyOrigin;
        private int editingAreaIndex = -1;

        /// <summary>
        /// 区画の制限高さを変更し、オブジェクトに反映するメソッド
        /// </summary>
        /// <param name="newHeight">新規に設定する制限高さ</param>
        public void ChangeHeight(float newHeight)
        {
            if (editingAreaProperty == null) return;

            newHeight = Mathf.Clamp(newHeight, 0, editingAreaProperty.wallMaxHeight);
            editingAreaProperty.limitHeight = newHeight;

             editingAreaProperty.SetLocalPosition(
                 new Vector3(
                 editingAreaProperty.transform.localPosition.x,
                 newHeight,
                 editingAreaProperty.transform.localPosition.z
                 ));

            editingAreaProperty.wallMaterial.SetFloat("_DisplayRate", newHeight / editingAreaProperty.wallMaxHeight);
            editingAreaProperty.wallMaterial.SetFloat("_LineCount", newHeight / editingAreaProperty.lineOffset);
        }

        /// <summary>
        /// 編集の対象となる区画データを指定するメソッド
        /// </summary>
        /// <param name="targetAreaIndex">対象区画のAreaPropertyリストの要素番号(-1の場合は指定を解除する)</param>
        public void SetEditTarget(int targetAreaIndex)
        {
            if(targetAreaIndex == -1)
            {
                editingAreaProperty = null;
                editingAreaPropertyOrigin = null;
            }
            else
            {
                editingAreaPropertyOrigin = AreasDataComponent.GetSnapshotProperty(targetAreaIndex);
                editingAreaProperty = AreasDataComponent.GetProperty(targetAreaIndex);
            }

            editingAreaIndex = targetAreaIndex;
        }

        /// <summary>
        /// 区画の制限高さの最大値を取得するメソッド
        /// </summary>
        /// <returns>制限高さの最大値(編集対象未セット時はnullを返す)</returns>
        public float? GetMaxHeight()
        {
            if (editingAreaProperty == null) return null;
            return editingAreaProperty.wallMaxHeight;
        }

        /// <summary>
        /// 現在の制限高さ値を取得するメソッド
        /// </summary>
        /// <returns>現在の制限高さ値(編集対象未セット時はnullを返す)</returns>
        public float? GetLimitHeight()
        {
            if (editingAreaProperty == null) return null;
            return editingAreaProperty.limitHeight;
        }

        /// <summary>
        /// 現在の区画名を取得するメソッド
        /// </summary>
        /// <returns>現在の区画名(編集対象未セット時はnullを返す)</returns>
        public string GetAreaName()
        {
            if (editingAreaProperty == null) return null;
            return editingAreaProperty.name;
        }

        /// <summary>
        /// 区画の名前を変更するメソッド
        /// </summary>
        /// <param name="newName"> 新しいエリア名 </param>
        public void ChangeAreaName(string newName)
        {
            if (editingAreaProperty == null) return;
            editingAreaProperty.name = newName;
        }

        /// <summary>
        /// 区画の色を取得するメソッド
        /// </summary>
        public Color GetColor()
        {
            if (editingAreaProperty == null) return Color.white;
            return editingAreaProperty.color;
        }

        /// <summary>
        /// 区画の色を変更するメソッド
        /// </summary>
        /// <param name="newColor"></param>
        public void ChangeColor(Color newColor)
        {
            if (editingAreaProperty == null) return;
            editingAreaProperty.color = newColor;
            editingAreaProperty.ceilingMaterial.SetColor("_Color", newColor);
            editingAreaProperty.wallMaterial.SetColor("_Color", newColor);
        }

        /// <summary>
        /// 対象区画の全プロパティを読み込み時の値に初期化するメソッド
        /// </summary>
        public void ResetProperty()
        {
            if(editingAreaIndex == -1) return;
            AreasDataComponent.TryResetProperty(editingAreaIndex);
        }

        /// <summary>
        /// 対象区画のプロパティを更新を確定されるメソッド
        /// </summary>
        public void ConfirmUpdatedProperty()
        {
            if (editingAreaIndex == -1) return;
            AreasDataComponent.TryUpdateSnapshotProperty(editingAreaIndex);
        }
    }
}
