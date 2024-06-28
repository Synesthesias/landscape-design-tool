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
        private static AddressableAssetSettings currentSettings;
        public static void LoadAndAddSettings()
        {
            // Addressableの取得
            currentSettings = AddressableAssetSettingsDefaultObject.Settings;
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
            if (!currentSettings.GetLabels().Contains("Plateau_Assets"))
            {
                currentSettings.AddLabel("Plateau_Assets");
            }
            if (!currentSettings.GetLabels().Contains("RuntimeTransformHandle_Assets"))
            {
                currentSettings.AddLabel("RuntimeTransformHandle_Assets");
            }
            if (!currentSettings.GetLabels().Contains("CustomPass"))
            {
                currentSettings.AddLabel("CustomPass");
            }

            AddGroupsToSettings();
        }

        private static void AddGroupsToSettings()
        {
            AddRuntimeHandleGroup();
            AddCustomPassGroup();
            AddPlateauAssetGroup();
        }
        private static void AddRuntimeHandleGroup()
        {
            var groupName = "RuntimeHandle";
            var targetGroup = CreateGroup(groupName);
            
            var guid = AssetDatabase.AssetPathToGUID("Packages/com.synesthesias.landscape-design-tool-2/Runtime/ArrangementAsset/Prefab/RuntimeTransformHandle.prefab");
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
            entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            entry.SetLabel("RuntimeTransformHandle_Assets", true);
        }
        private static void AddCustomPassGroup()
        {
            var groupName = "CustomPass";
            var targetGroup = CreateGroup(groupName);

            var guid = AssetDatabase.AssetPathToGUID("Packages/com.synesthesias.landscape-design-tool-2/Runtime/ArrangementAsset/Prefab/CustomPass.prefab");
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
            entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            entry.SetLabel("CustomPass", true);
        }
        private static void AddPlateauAssetGroup()
        {
            var groupName = "PlateauAssets";
            var targetGroup = CreateGroup(groupName);

            string propsDirectoryPath = "Assets/Samples/PLATEAU SDK-Toolkits for Unity/1.0.1/HDRP Sample Assets/Props/Prefabs";
            string[] propsAssetGUIDs = AssetDatabase.FindAssets("", new[] { propsDirectoryPath });
            foreach(var guid in propsAssetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
                entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                entry.SetLabel("Plateau_Assets", true);
            }

            string humansDirectoryPath = "Assets/Samples/PLATEAU SDK-Toolkits for Unity/1.0.1/HDRP Sample Assets/Humans/Prefabs";
            string[] humansAssetGUIDs = AssetDatabase.FindAssets("", new[] { humansDirectoryPath });
            foreach(var guid in humansAssetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
                entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                entry.SetLabel("Plateau_Assets", true);
            }
        }
        private static AddressableAssetGroup CreateGroup(string groupName)
        {
            var targetGroup = currentSettings.FindGroup(groupName);
            if(targetGroup != null)
            {
                return targetGroup;
            }
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
            return targetGroup;
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