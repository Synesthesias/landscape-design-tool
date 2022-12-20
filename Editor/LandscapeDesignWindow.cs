




// TODO　モック、あとで消す

// using System;
// using UnityEditor;
// using UnityEngine;
//
// namespace LandscapeDesignTool.Editor
// {
//     
//     [Obsolete]
//     public class LandscapeDesignWindow : EditorWindow
//     {
//         [MenuItem("PLATEAU/景観まちづくり/景観計画")]
//         public static void ShowWindow()
//         {
//             EditorWindow.GetWindow(typeof(LandscapeDesignWindow), true, "景観計画");
//         }
//
//         private void OnGUI()
//         {
//             DrawAddRegulationAreaPanel();
//             //DrawEditRegulationAreaPanel();
//             //DrawSimulateDesignPanel();
//             //DrawLoadShapeFilePanel();
//         }
//
//         private void DrawAddRegulationAreaPanel()
//         {
//             using (new EditorGUILayout.HorizontalScope())
//             {
//                 GUILayout.Label("規制タイプ選択");
//                 string[] regulationAreaTypes =
//                 {
//                     "規制エリア",
//                     "眺望規制"
//                 };
//                 EditorGUILayout.Popup(0, regulationAreaTypes);
//             }
//
//             GUILayout.Space(80);
//
//             GUILayout.Button("追加");
//
//         }
//
//         private void DrawEditRegulationAreaPanel()
//         {
//             EditorGUILayout.Toggle("画面表示", true);
//             using (new EditorGUILayout.HorizontalScope())
//             {
//                 GUILayout.Label("形状指定方法");
//                 string[] shapeType =
//                 {
//                     "多角形選択",
//                     "円形選択"
//                 };
//                 EditorGUILayout.Popup(0, shapeType);
//             }
//
//             GUILayout.Button("形状編集");
//             EditorGUILayout.FloatField("制限高さ", 50);
//             EditorGUILayout.FloatField("半径", 50);
//
//             GUILayout.Label("都市モデルへの反映", EditorStyles.boldLabel);
//             using (new EditorGUILayout.HorizontalScope())
//             {
//                 GUILayout.Button("制限高さでクリッピング");
//                 GUILayout.Button("クリッピングを解除");
//             }
//         }
//
//         private void DrawSimulateDesignPanel()
//         {
//             GUILayout.Label("建築物を選択してください");
//             GUILayout.Button("意匠変更");
//         }
//
//         private void DrawLoadShapeFilePanel()
//         {
//             GUILayout.Label("ファイル選択", EditorStyles.boldLabel);
//             using (new EditorGUILayout.HorizontalScope())
//             {
//                 GUILayout.TextField("");
//
//                 var skin = new GUIStyle(GUI.skin.button)
//                 {
//                     fixedWidth = 150f
//                 };
//                 GUILayout.Button("shpファイルを選択...", skin);
//             }
//
//             GUILayout.Space(10f);
//
//             GUILayout.Label("読み込み設定", EditorStyles.boldLabel);
//
//             using (new EditorGUILayout.HorizontalScope())
//             {
//                 GUILayout.Label("制限高さの属性");
//                 string[] shapeType =
//                 {
//                     "無し"
//                 };
//                 EditorGUILayout.Popup(0, shapeType);
//             }
//
//             GUILayout.Space(10f);
//
//             GUILayout.Button("読み込み");
//         }
//     }
// }
