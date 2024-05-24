using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;

namespace Landscape2.Editor
{
    public static class AddressableSettingsLoader
    {
        public static void LoadAndAddSettings()
        {
            // Addressableの取得
            var currentSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (currentSettings == null)
            {
                string settingsPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
                string directoryPath = Path.GetDirectoryName(settingsPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Debug.Log("Created directory: " + directoryPath);
                }
                AddressableAssetSettings initializeSettings = AddressableAssetSettings.Create(directoryPath, Path.GetFileNameWithoutExtension(settingsPath), true, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AddressableAssetSettingsDefaultObject.Settings = initializeSettings;
                currentSettings = AddressableAssetSettingsDefaultObject.Settings;
            }
            // ラベルの追加
            if (!currentSettings.GetLabels().Contains("PlateauProps_Assets"))
            {
                currentSettings.AddLabel("PlateauProps_Assets");
            }
            if (!currentSettings.GetLabels().Contains("RuntimeTransformHandle_Assets"))
            {
                currentSettings.AddLabel("RuntimeTransformHandle_Assets");
            }

            AddGroupsToSettings(currentSettings);
        }

        private static void AddGroupsToSettings(AddressableAssetSettings currentSettings)
        {
            AddRuntimeHandleGroup(currentSettings);
            AddPlateauAssetGroup(currentSettings);
        }
        private static void AddRuntimeHandleGroup(AddressableAssetSettings currentSettings)
        {
            var groupName = "RuntimeHandle";
            var targetGroup = currentSettings.FindGroup(groupName);
            if (targetGroup == null)
            {
                targetGroup = currentSettings.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema>(), typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
                var bundledAssetGroupSchema = targetGroup.GetSchema<BundledAssetGroupSchema>();
                if (bundledAssetGroupSchema != null)
                {
                    // Build & Load PathをLocalに変更
                    bundledAssetGroupSchema.BuildPath.SetVariableByName(currentSettings, AddressableAssetSettings.kLocalBuildPath);
                    bundledAssetGroupSchema.LoadPath.SetVariableByName(currentSettings, AddressableAssetSettings.kLocalLoadPath);
                    EditorUtility.SetDirty(targetGroup);
                    AssetDatabase.SaveAssets();
                }
            }
            var guid = AssetDatabase.AssetPathToGUID("Packages/com.synesthesias.landscape-design-tool-2/Runtime/ArrangementAsset/Prefab/RuntimeTransformHandle.prefab");
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
            entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            entry.SetLabel("RuntimeTransformHandle_Assets", true);
        }
        private static void AddPlateauAssetGroup(AddressableAssetSettings currentSettings)
        {
            //  アセットを取得
            string propsDirectoryPath = "Assets/Samples/PLATEAU SDK-Toolkits for Unity/1.0.1/HDRP Sample Assets/Props/Prefabs";
            string[] assetGUIDs = AssetDatabase.FindAssets("", new[] { propsDirectoryPath });

            var groupName = "PlateauAssets";
            var targetGroup = currentSettings.FindGroup(groupName);
            if (targetGroup == null)
            {
                targetGroup = currentSettings.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema>(), typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
                var bundledAssetGroupSchema = targetGroup.GetSchema<BundledAssetGroupSchema>();
                if (bundledAssetGroupSchema != null)
                {
                    // Build & Load PathをLocalに変更
                    bundledAssetGroupSchema.BuildPath.SetVariableByName(currentSettings, AddressableAssetSettings.kLocalBuildPath);
                    bundledAssetGroupSchema.LoadPath.SetVariableByName(currentSettings, AddressableAssetSettings.kLocalLoadPath);
                    EditorUtility.SetDirty(targetGroup);
                    AssetDatabase.SaveAssets();
                }
            }
            foreach(var guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
                entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                entry.SetLabel("PlateauProps_Assets", true);
            }
        }
    }
    
    
    public class CustomAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            AddressableSettingsLoader.LoadAndAddSettings();
        }
    }
}