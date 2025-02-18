using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Landscape2.Runtime
{
    /// <summary>
    /// データを管理するクラス
    /// </summary>
    public class LineOfSightDataComponent
    {
        [Serializable]
        public class PointData
        {
            public Vector3 pointPos;
            public float yOffset;
        }

        private Dictionary<string, PointData> viewPointDict;
        private Dictionary<string, PointData> landmarkDict;

        //private Dictionary<string, Vector3> viewPointDict;
        //private Dictionary<string, Vector3> landmarkDict;
        private Dictionary<string, AnalyzeViewPointElements> analyzeViewPointDataDict;
        private Dictionary<string, AnalyzeLandmarkElements> analyzeLandmarkDataDict;
        private LineOfSightUI lineOfSightUI;

        public LineOfSightDataComponent()
        {
            viewPointDict = new();
            landmarkDict = new();

            analyzeViewPointDataDict = new Dictionary<string, AnalyzeViewPointElements>();
            analyzeLandmarkDataDict = new Dictionary<string, AnalyzeLandmarkElements>();
        }

        /// <summary>
        /// 視点場、眺望対象を辞書に追加する
        /// offset高さも保持する様にした
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pointName"></param>
        /// <param name="pointData"></param>
        /// <returns></returns>
        public bool AddPointDict(LineOfSightType type, string pointName, PointData pointData)
        {
            if (type == LineOfSightType.viewPoint)
            {
                if (viewPointDict.ContainsKey(pointName))
                {
                    return false;
                }
                viewPointDict.Add(pointName, pointData);
                return true;
            }
            else if (type == LineOfSightType.landmark)
            {
                if (landmarkDict.ContainsKey(pointName))
                {
                    return false;
                }
                landmarkDict.Add(pointName, pointData);
                return true;
            }
            return false;

        }

        /// <summary>
        /// 視点場、眺望対象を辞書に追加する
        /// </summary>
        // public bool AddPointDict(LineOfSightType lineOfSightType, string pointName, Vector3 pointPos)
        // {
        //     if (lineOfSightType == LineOfSightType.viewPoint)
        //     {
        //         if (viewPointDict.ContainsKey(pointName))
        //         {
        //             return false;
        //         }
        //         viewPointDict.Add(pointName, pointPos);
        //         return true;
        //     }
        //     else if (lineOfSightType == LineOfSightType.landmark)
        //     {
        //         if (landmarkDict.ContainsKey(pointName))
        //         {
        //             return false;
        //         }
        //         landmarkDict.Add(pointName, pointPos);
        //         return true;
        //     }
        //     return false;
        // }

        /// <summary>
        /// 視点場解析結果を辞書に追加する
        /// </summary>
        public bool AddAnalyzeViewPoinDatatDict(string keyName, AnalyzeViewPointElements elements)
        {

            if (!analyzeViewPointDataDict.ContainsKey(keyName))
            {
                analyzeViewPointDataDict.Add(keyName, elements);
                return true;
            }
            else
            {
                Debug.LogWarning($"analyzeViewPointDataDict already has Key : {keyName}");
                return false;
            }
        }

        /// <summary>
        /// 眺望対象解析結果を辞書に追加する
        /// </summary>
        public bool AnddAnalyzeLandmarkDataDict(string keyName, AnalyzeLandmarkElements elements)
        {
            if (!analyzeLandmarkDataDict.ContainsKey(keyName))
            {
                analyzeLandmarkDataDict.Add(keyName, elements);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 視点場、眺望対象の辞書を取得する
        /// </summary>
        public Dictionary<string, PointData> GetPointDict(LineOfSightType type)
        {
            if (type == LineOfSightType.viewPoint)
            {
                return viewPointDict;
            }
            else if (type == LineOfSightType.landmark)
            {
                return landmarkDict;
            }
            return null;

        }

        /// <summary>
        /// 視点場、眺望対象の辞書を取得する
        /// </summary>
        // public Dictionary<string, Vector3> GetPointDict(LineOfSightType lineOfSightType)
        // {
        //     if (lineOfSightType == LineOfSightType.viewPoint)
        //     {
        //         return viewPointDict;
        //     }
        //     else if (lineOfSightType == LineOfSightType.landmark)
        //     {
        //         return landmarkDict;
        //     }
        //     return null;
        // }

        /// <summary>
        /// 視点場解析結果の辞書を取得する
        /// </summary>
        public Dictionary<string, AnalyzeViewPointElements> GetAnalyzeViewPoinDatatDict()
        {
            return analyzeViewPointDataDict;
        }

        /// <summary>
        /// 眺望対象解析結果の辞書を取得する
        /// </summary>
        public Dictionary<string, AnalyzeLandmarkElements> GetAnalyzeLandmarkDataDict()
        {
            return analyzeLandmarkDataDict;
        }

        /// <summary>
        /// 視点場、眺望対象を削除する
        /// </summary>
        public (bool isRemoved, List<string> removedAnalyzeKeyNameList) RemovePointElement(LineOfSightType lineOfSightType, string keyName)
        {
            var removedAnalyzeKeyNameList = new List<string>();
            if (lineOfSightType == LineOfSightType.viewPoint)
            {
                if (viewPointDict.ContainsKey(keyName))
                {
                    viewPointDict.Remove(keyName);
                    // 削除した視点場が視点場解析結果に含まれていた時の処理
                    foreach (KeyValuePair<string, AnalyzeViewPointElements> point in analyzeViewPointDataDict)
                    {
                        if (point.Value.startPosName == keyName)
                        {
                            removedAnalyzeKeyNameList.Add(point.Key);
                        }
                    }
                    foreach (string analyzeKeyName in removedAnalyzeKeyNameList)
                    {
                        RemoveAnalyzeViewPointElement(analyzeKeyName);
                    }
                    return (true, removedAnalyzeKeyNameList);
                }
            }
            if (lineOfSightType == LineOfSightType.landmark)
            {
                if (landmarkDict.ContainsKey(keyName))
                {
                    landmarkDict.Remove(keyName);
                    // 削除した眺望対象が視点場解析結果に含まれていた時の処理
                    foreach (KeyValuePair<string, AnalyzeViewPointElements> point in analyzeViewPointDataDict)
                    {
                        if (point.Value.endPosName == keyName)
                        {
                            removedAnalyzeKeyNameList.Add(point.Key);
                        }
                    }
                    foreach (string analyzeKeyName in removedAnalyzeKeyNameList)
                    {
                        RemoveAnalyzeViewPointElement(analyzeKeyName);
                    }
                    // 削除した眺望対象が眺望対象解析結果に含まれていた時の処理
                    foreach (KeyValuePair<string, AnalyzeLandmarkElements> point in analyzeLandmarkDataDict)
                    {
                        if (point.Value.startPosName == keyName && !removedAnalyzeKeyNameList.Contains(point.Key))
                        {
                            removedAnalyzeKeyNameList.Add(point.Key);
                        }
                    }
                    foreach (string analyzeKeyName in removedAnalyzeKeyNameList)
                    {
                        RemoveAnalyzeLandmarkElement(analyzeKeyName);
                    }
                    return (true, removedAnalyzeKeyNameList);
                }
            }
            return (false, removedAnalyzeKeyNameList);
        }

        /// <summary>
        /// 視点場解析結果を削除する
        /// </summary>
        public bool RemoveAnalyzeViewPointElement(string keyName)
        {
            if (analyzeViewPointDataDict.ContainsKey(keyName))
            {
                analyzeViewPointDataDict.Remove(keyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 眺望対象解析結果を削除する
        /// </summary>
        public bool RemoveAnalyzeLandmarkElement(string keyName)
        {
            if (analyzeLandmarkDataDict.ContainsKey(keyName))
            {
                analyzeLandmarkDataDict.Remove(keyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 辞書を初期化する
        /// </summary> 
        public void ClearDict(LineOfSightType lineOfSightType)
        {
            if (lineOfSightType == LineOfSightType.viewPoint)
            {
                viewPointDict.Clear();
            }
            else if (lineOfSightType == LineOfSightType.landmark)
            {
                landmarkDict.Clear();
            }
            else if (lineOfSightType == LineOfSightType.analyzeViewPoint)
            {
                analyzeViewPointDataDict.Clear();
            }
            else if (lineOfSightType == LineOfSightType.analyzeLandmark)
            {
                analyzeLandmarkDataDict.Clear();
            }
        }
    }
}
