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
            string path = "Packages/com.synesthesias.landscape-design-tool-2/Editor/PlateauProps_Assets.asset";
            var group = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>(path);

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

            var guid = AssetDatabase.AssetPathToGUID("Packages/com.synesthesias.landscape-design-tool-2/Runtime/ArrangementAsset/Prefab/RuntimeTransformHandle.prefab");
            var defaultGroup = currentSettings.DefaultGroup;
            var entry = currentSettings.CreateOrMoveEntry(guid, defaultGroup);
            entry.address = "RuntimeTransformHandleScriptObject";


            AddGroupsToSettings(currentSettings, group);
        }

        private static void AddGroupsToSettings(AddressableAssetSettings currentSettings, AddressableAssetGroup group)
        {
            var groupName = group.Name;
            var assetEntries = new List<AddressableAssetEntry>(group.entries);
            var targetGroup = currentSettings.FindGroup(groupName);
            if (targetGroup == null)
            {
                targetGroup = currentSettings.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema>(),typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            }
            if (groupName != "Built In Data")
            {
                foreach (var entry in assetEntries)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);
                    var newEntry = currentSettings.CreateOrMoveEntry(guid, targetGroup);
                    newEntry.address = entry.address;

                    if (entry.labels.Contains("PlateauProps_Assets"))
                    {
                        newEntry.SetLabel("PlateauProps_Assets", true);
                    }
                }
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