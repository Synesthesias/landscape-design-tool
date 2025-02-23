using Landscape2.Runtime.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace Landscape2.Runtime
{
    public static class ProjectSaveDataManager
    {
        // プロジェクト設定
        public static ProjectSetting ProjectSetting = new();
        
        // 配置アセット
        private static ProjectSaveData_Asset SaveDataAsset = new();
        
        // カメラ視点一覧
        private static ProjectSaveData_CameraPosition SaveDataCameraPosition = new();
        
        // 色彩編集した建築物一覧
        private static ProjectSaveData_EditBuilding SaveDataEditBuilding = new();
        
        // GISデータ一覧
        private static ProjectSaveData_GisData SaveDataGisData = new();
        
        // 景観計画区域一覧
        private static ProjectSaveData_LandscapePlan SaveDataLandscapePlan = new();
        
        // 見通し解析一覧
        private static ProjectSaveData_LineOfSight SaveDataLineOfSight = new();
        
        // BIMインポートして配置した建物一覧
        private static ProjectSaveData_BimImport SaveDataBimImport = new();
        
        
        // 編集イベント
        public static UnityEvent<string> OnEditProject = new();

        /// <summary>
        /// プロジェクトに追加
        /// </summary>
        public static void Add(ProjectSaveDataType dataType, string id, string projectID = "", bool isEdit = true)
        {
            projectID = string.IsNullOrEmpty(projectID) ? ProjectSetting.CurrentProject.projectID : projectID;
            
            switch (dataType)
            {
                case ProjectSaveDataType.Asset:
                    SaveDataAsset.Add(projectID, id);
                    break;
                case ProjectSaveDataType.CameraPosition:
                    SaveDataCameraPosition.Add(projectID, id);
                    break;
                case ProjectSaveDataType.EditBuilding:
                    SaveDataEditBuilding.Add(projectID, id);
                    break;
                case ProjectSaveDataType.GisData:
                    SaveDataGisData.Add(projectID, id);
                    break;
                case ProjectSaveDataType.LandscapePlan:
                    SaveDataLandscapePlan.Add(projectID, id);
                    break;
                case ProjectSaveDataType.LineOfSight:
                    SaveDataLineOfSight.Add(projectID, id);
                    break;
                case ProjectSaveDataType.BimImport:
                    SaveDataBimImport.Add(projectID, id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }

            if (isEdit)
            {
                Edit(dataType, id);
            }
        }

        /// <summary>
        /// 現在のプロジェクトから削除
        /// </summary>
        public static void Delete(ProjectSaveDataType dataType, string id)
        {
            Edit(dataType, id);
            switch (dataType)
            {
                case ProjectSaveDataType.Asset:
                    SaveDataAsset.Delete(id);
                    break;
                case ProjectSaveDataType.CameraPosition:
                    SaveDataCameraPosition.Delete(id);
                    break;
                case ProjectSaveDataType.EditBuilding:
                    SaveDataEditBuilding.Delete(id);
                    break;
                case ProjectSaveDataType.GisData:
                    SaveDataGisData.Delete(id);
                    break;
                case ProjectSaveDataType.LandscapePlan:
                    SaveDataLandscapePlan.Delete(id);
                    break;
                case ProjectSaveDataType.LineOfSight:
                    SaveDataLineOfSight.Delete(id);
                    break;
                case ProjectSaveDataType.BimImport:
                    SaveDataBimImport.Delete(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }

        /// <summary>
        /// 編集中に
        /// </summary>
        public static void Edit(ProjectSaveDataType dataType, string id)
        {
            var projectID = GetProjectID(dataType, id);
            
            // プロジェクトデータを編集中に
            OnEditProject.Invoke(projectID);
        }

        private static string GetProjectID(ProjectSaveDataType dataType, string id)
        {
            return dataType switch
            {
                ProjectSaveDataType.Asset => SaveDataAsset.GetProjectID(id),
                ProjectSaveDataType.CameraPosition => SaveDataCameraPosition.GetProjectID(id),
                ProjectSaveDataType.EditBuilding => SaveDataEditBuilding.GetProjectID(id),
                ProjectSaveDataType.GisData => SaveDataGisData.GetProjectID(id),
                ProjectSaveDataType.LandscapePlan => SaveDataLandscapePlan.GetProjectID(id),
                ProjectSaveDataType.LineOfSight => SaveDataLineOfSight.GetProjectID(id),
                ProjectSaveDataType.BimImport => SaveDataBimImport.GetProjectID(id),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }

        /// <summary>
        /// IDが存在するか確認
        /// </summary>
        public static bool TryCheckData(ProjectSaveDataType dataType, string projectID, string id, bool isCheckEditMode = true)
        {
            if (isCheckEditMode && !ProjectSetting.IsEditMode)
            {
                // 閲覧モードであればチェックを通さない
                return false;
            }

            return dataType switch
            {
                ProjectSaveDataType.Asset => SaveDataAsset.TryCheckData(projectID, id),
                ProjectSaveDataType.CameraPosition => SaveDataCameraPosition.TryCheckData(projectID, id),
                ProjectSaveDataType.EditBuilding => SaveDataEditBuilding.TryCheckData(projectID, id),
                ProjectSaveDataType.GisData => SaveDataGisData.TryCheckData(projectID, id),
                ProjectSaveDataType.LandscapePlan => SaveDataLandscapePlan.TryCheckData(projectID, id),
                ProjectSaveDataType.LineOfSight => SaveDataLineOfSight.TryCheckData(projectID, id),
                ProjectSaveDataType.BimImport => SaveDataBimImport.TryCheckData(projectID, id),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }
    }
}