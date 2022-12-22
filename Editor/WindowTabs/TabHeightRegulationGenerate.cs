using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabHeightRegulationGenerate
    {
        float _heightAreaHeight = 30.0f;
        float _heightAreaRadius = 100.0f;
        
        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>高さ規制エリア作成</size>", labelStyle);
            EditorGUILayout.HelpBox("高さ規制リアの高さ半径を設定しタイプを選択して規制エリア作成をクリックしてください", MessageType.Info);
            _heightAreaHeight = EditorGUILayout.FloatField("高さ", _heightAreaHeight);
            _heightAreaRadius = EditorGUILayout.FloatField("半径", _heightAreaRadius);

            if (GUILayout.Button("高さ規制エリア作成"))
            {
                GameObject grp = GameObject.Find("HeitRegurationAreaGroup");
                if (!grp)
                {
                    grp = new GameObject();
                    grp.name = "HeightRegurationArea";
                    grp.layer = LayerMask.NameToLayer("RegulationArea");

                    HeightRegurationAreaHandler handler = grp.AddComponent<HeightRegurationAreaHandler>();
                    handler.areaHeight = _heightAreaHeight;
                    handler.areaRadius = _heightAreaRadius;
                }
            }
        }
    }
}