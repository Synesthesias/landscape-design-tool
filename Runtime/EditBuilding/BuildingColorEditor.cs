using System.Collections;
using System.Collections.Generic;
using Landscape2.Runtime.LandscapePlanLoader;
using PLATEAU.CityInfo;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 建築物の色彩を編集
    /// UIは<see cref="BuildingColorEditorUI"/>が担当
    /// </summary>
    /// 
    public class BuildingColorEditor : ISubComponent
    {
        // 色彩を変更する対象のオブジェクト
        GameObject targetObject;
        // 編集中のマテリアル
        List<Material> editingMaterials = new List<Material>();

        // 地物型のマテリアルを管理するリスト
        List<Material> buildingFieldMaterials = new List<Material>();
        // Renderer.materialによって複製されたマテリアルを管理するリスト
        List<Material> copiedMaterials = new List<Material>();

        // UIで色を変更したときに呼び出される
        // 建築物のRGB値を変更
        public void EditMaterialColor(Color color)
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
        public int SetMaterialList(GameObject targetObject)
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
            // 編集中のマテリアルをリストに追加
            ChangeEditingMaterial(-1);

            // マテリアル分けされているかそうでないかでリストの要素数を変更
            if (buildingFieldMaterials.Count == 1)
            {
                return 1;
            }
            else
            {
                // buildingFieldMaterialsの先頭2つ(壁面，屋根面)以降の要素を削除
                buildingFieldMaterials.RemoveRange(2, buildingFieldMaterials.Count - 2);
                return 3;
            }
        }

        // 地物型選択リストが変更された時に呼び出される
        // 編集中のマテリアルを変更
        public void ChangeEditingMaterial(int id)
        {
            editingMaterials.Clear();

            // "要素全体"が選択された場合，全てのマテリアルを編集対象にする
            if (id == 0)
            {
                editingMaterials.AddRange(buildingFieldMaterials);
            }
            else if(id > 0)
            {
                // 選択された要素のみを編集対象にする
                editingMaterials.Add(buildingFieldMaterials[id - 1]);
            }
        }

        // マテリアルのSmoothnessを変更
        public void EditMaterialSmoothness(float value)
        {
            // 選択中のマテリアルのSmoothnessを変更
            if (editingMaterials.Count != 0)
            {
                foreach (var mat in editingMaterials)
                {
                    mat.SetFloat("_Smoothness", value);
                }
            }            
        }
        
        // 選択された要素の色を取得
        public Color GetMaterialColor()
        {
            if (editingMaterials.Count == 2) // 要素全体
            {
                if (editingMaterials[0].color == editingMaterials[1].color)
                {
                    return editingMaterials[0].color;
                }
            }
            else if(editingMaterials.Count == 1) // 壁面，屋根面
            {
                return editingMaterials[0].color;
            }
            // 未選択のとき，もしくは壁面と屋根面の色が異なる場合
            return Color.white;
        }

        // 選択された要素のSmoothnessを取得
        public float GetMaterialSmoothness()
        {

            if (editingMaterials.Count == 2) // 要素全体
            {
                if (editingMaterials[0].GetFloat("_Smoothness") == editingMaterials[1].GetFloat("_Smoothness"))
                {
                    return editingMaterials[0].GetFloat("_Smoothness");
                }
            }
            else if (editingMaterials.Count == 1) // 壁面，屋根面
            {
                return editingMaterials[0].GetFloat("_Smoothness");
            }
            // 未選択のとき，もしくは壁面と屋根面の色が異なる場合
            return 0f;
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
