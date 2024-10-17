using System;

namespace Landscape2.Runtime
{
    public enum ArrangementAssetType
    {
        Plant,
        Advertisement,
        Human,
        Vehicle,
        Building,
        StreetFurniture,
        Sign,
        Miscellaneous,
    }
    
    public static class ArrangementAssetTypeExtensions
    {
        public static string GetKeyName(this ArrangementAssetType type)
        {
            return type switch
            {
                ArrangementAssetType.Human => "Humans_Assets",
                ArrangementAssetType.Vehicle => "Vehicle_Assets",
                ArrangementAssetType.Building => "Buildings_Assets",
                ArrangementAssetType.Plant => "Plants_Assets",
                ArrangementAssetType.Advertisement => "Advertisements_Assets",
                ArrangementAssetType.StreetFurniture => "StreetFurnitures_Assets",
                ArrangementAssetType.Sign => "Signs_Assets",
                ArrangementAssetType.Miscellaneous => "Miscellaneous_Assets",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public static string GetButtonName(this ArrangementAssetType type)
        {
            return type switch
            {
                ArrangementAssetType.Human => "AssetCategory_Human",
                ArrangementAssetType.Vehicle => "AssetCategory_Vehicle",
                ArrangementAssetType.Building => "AssetCategory_Building",
                ArrangementAssetType.Plant => "AssetCategory_Tree",
                ArrangementAssetType.Advertisement => "AssetCategory_Ad",
                ArrangementAssetType.StreetFurniture => "AssetCategory_Light",
                ArrangementAssetType.Sign => "AssetCategory_Sign",
                ArrangementAssetType.Miscellaneous => "AssetCategory_Other",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static string GetCategoryName(this ArrangementAssetType type)
        {
            return type switch
            {
                ArrangementAssetType.Human => "人物",
                ArrangementAssetType.Vehicle => "車両",
                ArrangementAssetType.Building => "建物",
                ArrangementAssetType.Plant => "街路樹・植栽",
                ArrangementAssetType.Advertisement => "広告",
                ArrangementAssetType.StreetFurniture => "路上設備",
                ArrangementAssetType.Sign => "標識・標示",
                ArrangementAssetType.Miscellaneous => "その他",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}