using System;
using UnityEditor;
using UnityEngine;

namespace Landscape2.Runtime
{
    /// <summary>
    /// 汎用Json保存データ
    /// </summary>
    [Serializable]
    public class ArrangementJsonSaveData
    {
        [SerializeField] private string jsonData;
        public string JsonData => jsonData;

        public void Save(string jsonData)
        {
            this.jsonData = jsonData;
        }
    }
}