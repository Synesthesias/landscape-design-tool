using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor
{
    [CustomEditor(typeof(RegulationArea))]
    public class RegulationAreaEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Toggle("画面表示", true);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("形状指定方法");
                string[] shapeType =
                {
                    "多角形選択",
                    "円形選択"
                };
                EditorGUILayout.Popup(1, shapeType);
            }

            //GUILayout.Button("形状編集");
            EditorGUILayout.FloatField("制限高さ", 50);
            EditorGUILayout.FloatField("半径", 50);

            GUILayout.Label("都市モデルへの反映", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Button("制限高さでクリッピング");
                GUILayout.Button("クリッピングを解除");
            }
        }

        private void OnSceneGUI()
        {
            Handles.PositionHandle(new Vector3(1563, 7.7f, 238), Quaternion.identity);
        }
    }
}
