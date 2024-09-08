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
            AddLabels();
            AddGroupsToSettings();
        }
        private static void AddLabels()
        {
            AddLabel("Plateau_Assets");
            AddLabel("Tree_Assets");
            AddLabel("Advertisement_Assets");
            AddLabel("Humans_Assets");
            AddLabel("Vehicle_Assets");
            AddLabel("Information_Assets");
            AddLabel("StreetLight_Assets");
            AddLabel("RoadSign_Assets");
            AddLabel("PublicFacilities_Assets");
            AddLabel("Other_Assets");
            AddLabel("RuntimeTransformHandle_Assets");
            AddLabel("CustomPass");
            AddLabel("AssetsPicture");
        }
        private static void AddLabel(string labelName)
        {
            if(!currentSettings.GetLabels().Contains(labelName))
            {
                currentSettings.AddLabel(labelName);
            }
        }

        private static void AddGroupsToSettings()
        {
            // 第一引数 : グループ名、第二引数 : パス 
            AddAssetGroup("RuntimeTransformHandle_Assets","Packages/com.synesthesias.landscape-design-tool-2/Runtime/ArrangementAsset/Prefab/RuntimeTransformHandle.prefab");
            AddAssetGroup("CustomPass","Packages/com.synesthesias.landscape-design-tool-2/Runtime/ArrangementAsset/Prefab/CustomPass.prefab");
            AddAssetsPictureGroup("AssetsPicture","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Picture");
            AddPlateauAssetGroup("Tree_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Props/Prefabs/Tree");
            AddPlateauAssetGroup("Advertisement_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Props/Prefabs/Advertisement");
            AddPlateauAssetGroup("Humans_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Humans/Prefabs");
            AddPlateauAssetGroup("Vehicle_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Vehicles/Prefabs");
            AddPlateauAssetGroup("Information_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Props/Prefabs/Information");
            AddPlateauAssetGroup("StreetLight_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Props/Prefabs/StreetLight");
            AddPlateauAssetGroup("RoadSign_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Props/Prefabs/RoadSign");
            AddPlateauAssetGroup("PublicFacilities_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Props/Prefabs/PublicFacilities");
            AddPlateauAssetGroup("Other_Assets","Packages/com.synesthesias.landscape-design-tool-2/HDRP Sample Assets/Props/Prefabs/Others");
        }

        private static void AddAssetGroup(string groupName,string path)
        {
            var targetGroup = CreateGroup(groupName);
            var guid =  AssetDatabase.AssetPathToGUID(path);
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
            entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            entry.SetLabel(groupName, true);
        }

        private static void AddAssetsPictureGroup(string groupName,string directoryPath)
        {
            var targetGroup = CreateGroup(groupName);
            string[] picturesGUIDs = AssetDatabase.FindAssets("t:Texture2D", new[] { directoryPath });

            foreach(var guid in picturesGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
                entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                entry.SetLabel(groupName, true);
            }
        }
        private static void AddPlateauAssetGroup(string groupName,string directoryPath)
        {
            var targetGroup = CreateGroup(groupName);
            string[] propsAssetGUIDs = AssetDatabase.FindAssets("", new[] { directoryPath });

            foreach(var guid in propsAssetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var entry = currentSettings.CreateOrMoveEntry(guid,targetGroup);
                entry.address = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                entry.SetLabel(groupName, true);
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