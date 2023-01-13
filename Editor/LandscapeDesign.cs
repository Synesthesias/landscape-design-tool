using LandscapeDesignTool.Editor.WindowTabs;
using UnityEngine;
using UnityEditor;


namespace LandscapeDesignTool.Editor
{
#if UNITY_EDITOR
    public class LandscapeDesign : EditorWindow
    {

        private readonly string[] _tabToggles =
            { "視点場作成", "規制エリア作成", "眺望規制作成", "高さ規制エリア作成", "ShapeFile読込", "ShapeFile書き出し" };

        private int _tabIndex;
        private readonly TabViewPointGenerate _tabViewPointGenerate;
        private readonly TabRegulationAreaGenerate _tabRegulationAreaGenerate;
        private readonly TabShapefileLoad _tabShapefileLoad = new TabShapefileLoad();
        private readonly TabHeightRegulationGenerate _tabHeightRegulationGenerate = new TabHeightRegulationGenerate();
        private readonly TabRegulationAreaExport _tabRegulationAreaExport = new TabRegulationAreaExport();

        private readonly TabViewportRegulationGenerate _tabViewportRegulationGenerate =
            new TabViewportRegulationGenerate();

        public LandscapeDesign()
        {
            _tabViewPointGenerate = new TabViewPointGenerate(this);
            _tabRegulationAreaGenerate = new TabRegulationAreaGenerate(this);
        }

        [MenuItem("PLATEAU/景観まちづくり/景観計画")]
        public static void ShowWindow()
        {
            TagAdder.ConfigureTags();
            EditorWindow.GetWindow(typeof(LandscapeDesign), true, "景観計画画面");
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            switch (_tabIndex)
            {
                case 0:
                    //_tabViewPointGenerate.OnSceneGUI();
                    break;
                case 1:
                    _tabRegulationAreaGenerate.OnSceneGUI();
                    break;
                case 2:
                    _tabViewportRegulationGenerate.OnSceneGUI();
                    break;
                case 3:
                    _tabHeightRegulationGenerate.OnSceneGUI();
                    break;
            }
        }

        private void Update()
        {
            _tabViewPointGenerate.Update();
        }

        private void OnGUI()
        {

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _tabIndex = GUILayout.Toolbar(_tabIndex, _tabToggles, new GUIStyle(EditorStyles.toolbarButton),
                    GUI.ToolbarButtonSize.FitToContents);
            }

            switch (_tabIndex)
            {
                case 0:
                    _tabViewPointGenerate.Draw();
                    break;
                case 1:
                    _tabRegulationAreaGenerate.Draw();
                    break;
                case 2:
                    _tabViewportRegulationGenerate.Draw();
                    break;
                case 3:
                    _tabHeightRegulationGenerate.Draw();
                    break;
                case 4:
                    _tabShapefileLoad.Draw();
                    break;
                case 5:
                    _tabRegulationAreaExport.Draw();
                    break;
            }
        }
    }
}


#endif