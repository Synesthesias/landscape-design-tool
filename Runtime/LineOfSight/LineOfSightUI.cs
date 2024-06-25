using Landscape2.Runtime.LineOfSight;
using Landscape2.Runtime.UiCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class LineOfSightUI : ISubComponent
    {
        private const string UxmlFileName = "LineOfSightUI";
        

        public static LineOfSightUI CreateForScene()
        {
            var ui = new UIDocumentFactory().CreateWithUxmlName(UxmlFileName);
            return new LineOfSightUI(ui);
        }

        public static LineOfSightUI CreateForEditorWindow(VisualElement rootVisualElement)
        {
            // Uxmlファイルのうち、ウィンドウの位置に関わるRootは除いて、内容のRootのみを追加することでEditorWindowの上から表示されるようにします。
            var visualTree = Resources.Load<VisualTreeAsset>(UxmlFileName);
            var cloned = visualTree.CloneTree();
            var targetElement = cloned.Q("LineOfSightUiContentRoot");
            rootVisualElement.Add(targetElement);
            return new LineOfSightUI(rootVisualElement);
        }

        private LineOfSightUI(VisualElement uiRoot)
        {
            var createButton = uiRoot.Q<Button>("LineOfSightCreateButton");
            createButton.clicked += CreateButtonPushed;
        }

        private void CreateButtonPushed()
        {
            var grp = new GameObject
            {
                name = "LineOfSight",
                // layer = LayerMask.NameToLayer("RegulationArea")
            };
            // 視線を生成します。
            LineOfSight.LineOfSight handler = grp.AddComponent<LineOfSight.LineOfSight>();
            handler.UpdateParams(80, 80, new Vector3(100,0,0) );
            #if UNITY_EDITOR
            EditorUtility.SetDirty(handler);
            #endif
        }

        public void Update(float deltaTime)
        {
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void Start()
        {
        }
    }
}
