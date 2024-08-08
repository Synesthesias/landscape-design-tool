using System.Collections;
using System.Collections.Generic;
using PLATEAU.CityInfo;
using UnityEngine;
using static Landscape2.Runtime.ColorEditorUI;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 建築物の色彩を変更
    /// UIは<see cref="BuildingColorEditorUI"/>が担当
    /// </summary>
    public class BuildingColorEditor : ISubComponent
    {
        // 色彩を変更する対象のオブジェクト
        GameObject targetObject;
        // 編集中のマテリアル
        List<Material> editingMaterials = new List<Material>();

        BuildingColorEditorUI buildingColorEditorUI;
        EditBuilding editBuilding;

        // 地物型のマテリアルを管理するリスト
        List<Material> buildingFieldMaterials = new List<Material>();
        // Renderer.materialによって複製されたマテリアルを管理するリスト
        List<Material> copiedMaterials = new List<Material>();

        public BuildingColorEditor(BuildingColorEditorUI buildingColorEditorUI,EditBuilding editBuilding)
        {
            this.buildingColorEditorUI = buildingColorEditorUI;
            this.editBuilding = editBuilding;

            // 色彩変更UIの色変更イベントに登録
            this.buildingColorEditorUI = buildingColorEditorUI;
            this.buildingColorEditorUI.OnColorEdited += EditMaterialColor;
            this.buildingColorEditorUI.OnFieldChanged += ChangeEditingMaterial;
            // 建物編集画面の建物選択イベントに登録
            this.editBuilding.OnBuildingSelected += SetMaterialList;
        }

        // UIで色を変更したときに呼び出される
        // 建築物のRGB値を変更
        private void EditMaterialColor(Color color)
        {
            if(targetObject != null)
            {
                // Renderer.materialにアクセスすることでマテリアルを複製し，元のMaterialを変更せずに色を変更する
                // 選択中のマテリアルの色を変更
                if (editingMaterials != null)
                {
                    // editingMaterialsの要素でループ
                    foreach (var mat in editingMaterials)
                    {
                        // 色を変更
                        mat.color = color;
                    }            
                }
            }
        }

        // 建物が選択されたときに呼び出される
        // 選択中のオブジェクトのマテリアルを編集中リストに設定
        private void SetMaterialList(GameObject targetObject)
        {
            buildingFieldMaterials.Clear();
            this.targetObject = targetObject;

            buildingFieldMaterials.AddRange(this.targetObject.GetComponent<MeshRenderer>().materials);

            foreach (var mat in editingMaterials)
            {
                // 編集中のマテリアルをリストに追加
                if (!copiedMaterials.Contains(mat))
                {
                    copiedMaterials.Add(mat);
                }
            }

            // マテリアル分けされているかそうでないかでリストの要素数を変更
            if (buildingFieldMaterials.Count == 1)
            {
                buildingColorEditorUI.SetFieldList(1);
            }
            else
            {
                // buildingFieldMaterialsの先頭2つ(壁面，屋根面)以降の要素を削除
                buildingFieldMaterials.RemoveRange(2, buildingFieldMaterials.Count - 2);
                buildingColorEditorUI.SetFieldList(3);
            }

            // 編集中のマテリアルをリストに追加
            ChangeEditingMaterial(0);
        }

        // 地物型選択リストが変更された時に呼び出される
        // 編集中のマテリアルを変更
        private void ChangeEditingMaterial(int id)
        {
            editingMaterials.Clear();

            // "要素全体"が選択された場合，全てのマテリアルを編集対象にする
            if (id == 0)
            {
                editingMaterials.AddRange(buildingFieldMaterials);
            }
            else
            {
                // 選択された要素のみを編集対象にする
                editingMaterials.Add(buildingFieldMaterials[id - 1]);
            }
        }

        public void UpdateBuildingColor()
        {
        }
        public void Start()
        {
        }
        public void Update(float deltaTime)
        {
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
            // copiedMaterialsのマテリアルを破棄
            foreach (var mat in copiedMaterials)
            {
                GameObject.Destroy(mat);
            }
            copiedMaterials.Clear();
            targetObject = null;
        }
    }
}
