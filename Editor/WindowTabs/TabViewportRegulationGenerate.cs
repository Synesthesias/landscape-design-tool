using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    public class TabViewportRegulationGenerate
    {
        private float _screenWidth = 80.0f;
        private float _screenHeight = 80.0f;
        private string _viewRegulationAreaObjName = "RagulationArea";
        private int _selectViewRegulationIdx;

        public void Draw(GUIStyle labelStyle)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=15>眺望対象からの眺望規制作成</size>", labelStyle);
            EditorGUILayout.HelpBox("眺望対象地点での幅と高さを設定し眺望規制作成をクリックしてください", MessageType.Info);

            _viewRegulationAreaObjName = EditorGUILayout.TextField("ゲームオブジェクト名", _viewRegulationAreaObjName);
            _screenWidth = EditorGUILayout.FloatField("眺望対象地点での幅", _screenWidth);
            _screenHeight = EditorGUILayout.FloatField("眺望対象地点での高さ", _screenHeight);

            if (GUILayout.Button("眺望規制作成"))
            {
                LDTTools.CheckLayers();
                
                var grp = new GameObject
                {
                    name = _viewRegulationAreaObjName,
                    layer = LayerMask.NameToLayer("RegulationArea")
                };

                ViewRegulation handler = grp.AddComponent<ViewRegulation>();
                handler.screenHeight = _screenHeight;
                handler.screenWidth = _screenWidth;
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("眺望規制選択");
            // TODO 毎フレーム検索はちょっと重い
            var viewRegulations = Object.FindObjectsOfType<ViewRegulation>();
            var viewRegulationOptions = viewRegulations.Select(v => v.name).ToArray();
            if (viewRegulations.Length == 0) return;
            _selectViewRegulationIdx = Math.Min(_selectViewRegulationIdx, viewRegulations.Length);
            _selectViewRegulationIdx = EditorGUILayout.Popup("眺望規制", _selectViewRegulationIdx, viewRegulationOptions);
            var selectedViewRegulation = viewRegulations[_selectViewRegulationIdx];
            
        }
    }
}