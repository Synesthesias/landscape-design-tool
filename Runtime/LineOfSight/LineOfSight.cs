using System;
using UnityEditor;
using UnityEngine;

namespace Landscape2.Runtime.LineOfSight
{
    // 前作では ViewRegulation という名前でした
    public class LineOfSight : MonoBehaviour
    {
        [SerializeField] float screenWidth = 80.0f;
        [SerializeField] float screenHeight = 80.0f;

        /// <summary> 視線の向かう先 </summary>
        [SerializeField] Vector3 endPos;
        [SerializeField] Vector3 startPos;

        [SerializeField] float lineInterval = 4;
        [SerializeField] Color lineColorValid = new Color(0, 1, 0, 0.2f);
        [SerializeField] Color lineColorInvalid = new Color(1, 0, 0, 0.2f);
        
        
        public float ScreenWidth
        {
            get => screenWidth;
            set => screenWidth = value;
        }
        public float ScreenHeight
        {
            get => screenHeight;
            set => screenHeight = value;
        }
        public Vector3 EndPos
        {
            get => endPos;
            set => endPos = value;
        }
        public Vector3 StartPos
        {
            get => startPos;
            set => startPos = value;
        }

        public Color LineColorValid
        {
            get => lineColorValid;
            set => lineColorValid = value;
        }
        public Color LineColorInvalid
        {
            get => lineColorInvalid;
            set => lineColorInvalid = value;
        }
        public float LineInterval
        {
            get => lineInterval;
            set => lineInterval = value;
        }


        public void UpdateParams(float screenWidthArg, float screenHeightArg, Vector3 endPosArg)
        {
            screenWidth = screenWidthArg;
            screenHeight = screenHeightArg;
            endPos = endPosArg;
        }
        
    }

    [CustomEditor(typeof(LineOfSight))]
    public class LineOfSightEditor : UnityEditor.Editor
    {
        private LineOfSightManipulator manipulator;
        private void OnEnable()
        {
            manipulator = new LineOfSightManipulator((LineOfSight)target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var los = (LineOfSight)target;
            manipulator.DrawEditorConfig(los);
        }
        
        public void OnSceneGUI()
        {
            var los = (LineOfSight)target;
            manipulator.OnSceneGUI(los);
        }
    }
}