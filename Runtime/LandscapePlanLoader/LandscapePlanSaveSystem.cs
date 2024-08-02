// using System;
// using System.Collections.Generic;
// using UnityEngine;

// namespace Landscape2.Runtime.LandscapePlanLoader
// {
//     /// <summary>
//     /// 景観区画のセーブデータ項目
//     /// </summary>
//     [Serializable]
//     public struct PlanAreaSaveData
//     {
//         [SerializeField] private int _id;
//         [SerializeField] private string _name;
//         [SerializeField] private float _limitHeight;
//         [SerializeField] private float _lineOffset;
//         [SerializeField] private Color _color;
//         [SerializeField] private float _wallMaxHeight;
//         [SerializeField] private List<Vector3> _pointData;

//         public int id => _id;
//         public string name => _name;
//         public float limitHeight => _limitHeight;
//         public float lineOffset => _lineOffset;
//         public Color color => _color;
//         public float wallMaxHeight => _wallMaxHeight;
//         public List<Vector3> pointData => _pointData;

//         public PlanAreaSaveData(int id, string name, float limitHeight, float lineOffset, Color color, float wallMaxHeight, List<Vector3> pointData)
//         {
//             _id = id;
//             _name = name;
//             _limitHeight = limitHeight;
//             _lineOffset = lineOffset;
//             _color = color;
//             _wallMaxHeight = wallMaxHeight;
//             _pointData = pointData;
//         }
//     }


//     /// <summary>
//     /// 景観区画の保存と読み込み時の処理を、save systemに登録するクラス
//     /// </summary>
//     public class LandscapePlanSaveSystem
//     {
//         private SaveSystem _saveSystem;
//         private LandscapePlanSaveLoadHandler saveLoadHandler;

//         /// <summary>
//         /// 保存と読み込み時の処理をSaveSystemに登録するメソッド
//         /// </summary>
//         public void InstantiateSaveSystem(SaveSystem saveSystem)
//         {
//             saveLoadHandler = new LandscapePlanSaveLoadHandler();
//             _saveSystem = saveSystem;
//             SetSaveMode();
//             SetLoadMode();
//         }

//         public void SetSaveMode()
//         {
//             _saveSystem.SaveEvent += saveLoadHandler.SaveInfo;
//         }

//         public void SetLoadMode()
//         {
//             _saveSystem.LoadEvent += saveLoadHandler.LoadInfo;
//         }
//     }
// }
