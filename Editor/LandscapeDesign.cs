using System.Collections;
using System.Collections.Generic;
using LandscapeDesignTool.Editor.WindowTabs;
using UnityEngine;
using UnityEditor;


namespace LandscapeDesignTool.Editor
{
#if UNITY_EDITOR
    public class LandscapeDesign : EditorWindow
    {
// <<<<<<< HEAD
        // int _regulationType;
        // float _regulationHeight = 10;
        // float _screenWidth = 80.0f;
        // float _screenHeight = 80.0f;
        // float _heightAreaHeight = 30.0f;
        // float _heightAreaRadius = 100.0f;
        //
        //
        // string _regulationAreaExportPath = "";
        // bool _regulationAreaEdit = false;
        // AnyPolygonRegurationAreaHandler polygonHandler;
        //
        // List<Vector3> vertex = new List<Vector3>();
// =======
// >>>>>>> main


        private readonly string[] _tabToggles =
            { "‹“_êì¬", "‹K§ƒGƒŠƒAì¬", "’­–]‹K§ì¬", "‚‚³‹K§ƒGƒŠƒAì¬", "ShapeFile“Ç", "ShapeFile‘‚«o‚µ" };

        private int _tabIndex;
        private readonly TabViewPointGenerate _tabViewPointGenerate = new TabViewPointGenerate();
        private readonly TabRegulationAreaGenerate _tabRegulationAreaGenerate;
        private readonly TabShapefileLoad _tabShapefileLoad = new TabShapefileLoad();
        private readonly TabHeightRegulationGenerate _tabHeightRegulationGenerate = new TabHeightRegulationGenerate();
        private readonly TabRegulationAreaExport _tabRegulationAreaExport = new TabRegulationAreaExport();

        private readonly TabViewportRegulationGenerate _tabViewportRegulationGenerate =
            new TabViewportRegulationGenerate();

        public LandscapeDesign()
        {
            _tabRegulationAreaGenerate = new TabRegulationAreaGenerate(this);
        }

        [MenuItem("PLATEAU/ŒiŠÏ‚Ü‚¿‚Ã‚­‚è/ŒiŠÏŒv‰æ")]
        public static void ShowWindow()
        {
            TagAdder.ConfigureTags();
            EditorWindow.GetWindow(typeof(LandscapeDesign), true, "ŒiŠÏŒv‰æ‰æ–Ê");
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
// <<<<<<< HEAD
            if (_tabIndex == 1)
            {
                _tabRegulationAreaGenerate.OnSceneGUI();
            }
            else if (_tabIndex == 3)
            {
                _tabHeightRegulationGenerate.OnSceneGUI();
            }
        }

// =======
        // _tabRegulationAreaGenerate.OnSceneGUI();
// >>>>>>> main

        private void OnGUI()
        {
            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;


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
                    _tabViewPointGenerate.Draw(style);
                    // _tabRegulationAreaGenerate.OnGUI(style);
                    break;
                case 1:
                    _tabRegulationAreaGenerate.OnGUI(style);
                    // _tabViewportRegulationGenerate.Draw(style);
                    break;
                case 2:
                    _tabViewportRegulationGenerate.Draw(style);
                    // _tabHeightRegulationGenerate.Draw(style);
                    break;
                case 3:
                    _tabHeightRegulationGenerate.Draw(style);
                    // _tabRegulationAreaExport.Draw(style);
                    break;
                case 4:
                    _tabShapefileLoad.Draw(style);
                    break;
                case 5:
                    _tabRegulationAreaExport.Draw(style);
                    break;
// >>>>>>> main
            }
        }
    }
}


#endif