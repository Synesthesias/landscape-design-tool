using System;
using System.Linq;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区域プランニング用のDBFフィールドマッピング設定を管理する構造体
    /// </summary>
    [Serializable]
    public struct AreaPlanningDbfFieldSettings
    {
        /// <summary>
        /// エリア名を取得するDBFフィールド名候補
        /// </summary>
        public string[] AreaNameFields;
        
        /// <summary>
        /// 色を取得するDBFフィールド名候補
        /// </summary>
        public string[] ColorFields;
        
        /// <summary>
        /// IDを取得するDBFフィールド名候補
        /// </summary>
        public string[] IdFields;
        
        /// <summary>
        /// 高さを取得するDBFフィールド名候補
        /// </summary>
        public string[] HeightFields;
        
        /// <summary>
        /// デフォルトのDBFフィールド名定数
        /// </summary>
        public static readonly string[] DefaultAreaNameFields = { "AREANAME", "名前", "NAME", "AREA_NAME" };
        public static readonly string[] DefaultColorFields = { "COLOR", "色", "COLOUR" };
        public static readonly string[] DefaultIdFields = { "ID", "識別子", "番号" };
        public static readonly string[] DefaultHeightFields = { "HEIGHT", "高さ", "MAX_HEIGHT" };

        /// <summary>
        /// DBFフィールドマッピング設定のコンストラクタ
        /// </summary>
        public AreaPlanningDbfFieldSettings(
            string[] areaNameFields = null,
            string[] colorFields = null,
            string[] idFields = null,
            string[] heightFields = null)
        {
            AreaNameFields = areaNameFields ?? DefaultAreaNameFields;
            ColorFields = colorFields ?? DefaultColorFields;
            IdFields = idFields ?? DefaultIdFields;
            HeightFields = heightFields ?? DefaultHeightFields;
        }

        /// <summary>
        /// エリア名フィールド候補を追加
        /// </summary>
        public AreaPlanningDbfFieldSettings AddAreaNameField(string fieldName)
        {
            return new AreaPlanningDbfFieldSettings(
                areaNameFields: (AreaNameFields ?? DefaultAreaNameFields).Append(fieldName).ToArray(),
                colorFields: ColorFields ?? DefaultColorFields,
                idFields: IdFields ?? DefaultIdFields,
                heightFields: HeightFields ?? DefaultHeightFields
            );
        }

        /// <summary>
        /// 色フィールド候補を追加
        /// </summary>
        public AreaPlanningDbfFieldSettings AddColorField(string fieldName)
        {
            return new AreaPlanningDbfFieldSettings(
                areaNameFields: AreaNameFields ?? DefaultAreaNameFields,
                colorFields: (ColorFields ?? DefaultColorFields).Append(fieldName).ToArray(),
                idFields: IdFields ?? DefaultIdFields,
                heightFields: HeightFields ?? DefaultHeightFields
            );
        }

        /// <summary>
        /// IDフィールド候補を追加
        /// </summary>
        public AreaPlanningDbfFieldSettings AddIdField(string fieldName)
        {
            return new AreaPlanningDbfFieldSettings(
                areaNameFields: AreaNameFields ?? DefaultAreaNameFields,
                colorFields: ColorFields ?? DefaultColorFields,
                idFields: (IdFields ?? DefaultIdFields).Append(fieldName).ToArray(),
                heightFields: HeightFields ?? DefaultHeightFields
            );
        }

        /// <summary>
        /// 高さフィールド候補を追加
        /// </summary>
        public AreaPlanningDbfFieldSettings AddHeightField(string fieldName)
        {
            return new AreaPlanningDbfFieldSettings(
                areaNameFields: AreaNameFields ?? DefaultAreaNameFields,
                colorFields: ColorFields ?? DefaultColorFields,
                idFields: IdFields ?? DefaultIdFields,
                heightFields: (HeightFields ?? DefaultHeightFields).Append(fieldName).ToArray()
            );
        }
    }
}