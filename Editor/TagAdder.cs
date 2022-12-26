using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LandscapeDesignTool.Editor
{
    /// <summary>
    /// 都市計画ツールを動作させるにあたって必要なタグ設定を行います。
    /// </summary>
    public static class TagAdder
    {
        private static readonly Func<Transform, bool> predIsBuilding = trans => trans.name.Contains("_bldg_");
        private static readonly Func<Transform, bool> predIsGround = trans => trans.name.Contains("_dem_");
        private const string LayerNameBuilding = "Building";
        private const string LayerNameGround = "Ground";
        private const int LayerIdBuilding = 29;
        private const int LayerIdGround = 31;

        /// <summary>
        /// プロジェクト設定のレイヤー設定で、Building, Ground を設定します。
        /// シーン中の名前に _bldg_ を含むゲームオブジェクトのタグを Building に設定します。
        /// シーン中の名前に _dem_  を含むゲームオブジェクトのタグを Ground   に設定します。
        /// </summary>
        public static void ConfigureTags()
        {
            EditorUtility.DisplayProgressBar("", "タグを設定中です...", 30f);
            try
            {
                ConfigureLayerName();
                SetTagOfBuildingAndGround();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
        private static void SetTagOfBuildingAndGround()
        {
            var targets = Search(trans => predIsBuilding(trans) || predIsGround(trans));
            foreach (var target in targets)
            {
                if (predIsBuilding(target))
                {
                    SetTagsRecursive(target, LayerIdBuilding);
                }else if (predIsGround(target))
                {
                    SetTagsRecursive(target, LayerIdGround);
                }
            }
        }

        /// <summary>
        /// 開いている各シーン中の各ルートオブジェクトについて、それらとその子を探索します。
        /// <paramref name="pred"/> に合致するものをリストで返します。
        /// ただし、 predに合致する GameObject の子は探索対象から外します。
        /// </summary>
        private static List<Transform> Search(Func<Transform, bool> pred)
        {
            int sceneCount = SceneManager.sceneCount;
            var ret = new List<Transform>();
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    SearchRecursive(root.transform, ret, pred);
                }
            }

            return ret;
        }

        private static void SearchRecursive(Transform trans, List<Transform> ret, Func<Transform, bool> pred)
        {
            if (pred(trans))
            {
                ret.Add(trans);
                // 条件に合致したものの子までは探索しません。
                return;
            }
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = trans.GetChild(i);
                SearchRecursive(child, ret, pred);
            }
        }

        private static void SetTagsRecursive(Transform trans, int layerId)
        {
            trans.gameObject.layer = layerId;
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = trans.GetChild(i);
                SetTagsRecursive(child, layerId);
            }
        }

        /// <summary>
        /// プロジェクト設定のレイヤー名を変更し、Building, Ground という名前のレイヤーを作ります。
        /// 参考: <see href="https://forum.unity.com/threads/adding-layer-by-script.41970/#post-2274824"/>
        /// </summary>
        private static void ConfigureLayerName()
        {
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                Debug.LogWarning(
                    "Can't set up the layers.  It's possible the format of the layers and tags data has changed in this version of Unity.");
                Debug.LogWarning("Layers is null: " + (layers == null));
                return;
            }

            var layersToSet = new (int id, string name)[]
            {
                (LayerIdBuilding, LayerNameBuilding), (LayerIdGround, LayerNameGround)
            };
            foreach (var layerTuple in layersToSet)
            {
                SerializedProperty layerProperty = layers.GetArrayElementAtIndex(layerTuple.id);
                if (layerProperty.stringValue != layerTuple.name)
                {
                    layerProperty.stringValue = layerTuple.name;
                }
            }
        }
    }
}
