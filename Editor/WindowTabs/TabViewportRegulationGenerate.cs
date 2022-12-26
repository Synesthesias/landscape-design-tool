using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabViewportRegulationGenerate
    {
        float _screenWidth = 80.0f;
        float _screenHeight = 80.0f;
        
        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>眺望対象からの眺望規制作成</size>", labelStyle);
            EditorGUILayout.HelpBox("眺望対象地点での幅と高さを設定し眺望規制作成をクリックしてください", MessageType.Info);

            _screenWidth = EditorGUILayout.FloatField("眺望対象地点での幅", _screenWidth);
            _screenHeight = EditorGUILayout.FloatField("眺望対象地点での高さ", _screenHeight);

            if (GUILayout.Button("眺望規制作成"))
            {
                LDTTools.CheckLayers();

                GameObject grp = GameObject.Find("RegurationArea");
                if (!grp)
                {
                    grp = new GameObject();
                    grp.name = "RegurationArea";
                    grp.layer = LayerMask.NameToLayer("RegulationArea");

                    ViewRegulation handler = grp.AddComponent<ViewRegulation>();
                    handler.screenHeight = _screenHeight;
                    handler.screenWidth = _screenWidth;

                }

            }
        }
    }
}