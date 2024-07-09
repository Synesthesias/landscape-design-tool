using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    #if UNITY_EDITOR
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
            DrawEditorConfig(los);
        }
        
        public void OnSceneGUI()
        {
            var los = (LineOfSight)target;
            manipulator.OnSceneGUI(los);
        }
        
        /// <summary>
        /// 視線規制の設定GUIを描画します。
        /// </summary>
        /// <returns>ユーザーがGUIで何らかの設定変更をしたときにtrueを返します。</returns>
        private bool DrawEditorConfig(LineOfSight target)
        {
            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            
            SceneView sceneView = SceneView.lastActiveSceneView;
            
            bool isGuiChanged = false;
            using (var checkChange = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.HelpBox("視点場を選択して眺望対象をシーン内で選択してください", MessageType.Info);
                target.ScreenWidth = EditorGUILayout.FloatField("眺望対象での横サイズ(m)", target.ScreenWidth);
                target.ScreenHeight = EditorGUILayout.FloatField("眺望対象での縦サイズ(m)", target.ScreenHeight);
                target.LineColorValid = EditorGUILayout.ColorField("色の設定", target.LineColorValid);
                target.LineColorInvalid = EditorGUILayout.ColorField("規制色の設定", target.LineColorInvalid);
                target.LineInterval = EditorGUILayout.FloatField("障害物の判定間隔(m)", target.LineInterval);
                isGuiChanged |= checkChange.changed;
            }
            
            //
            // EditorGUILayout.Space();
            // EditorGUILayout.LabelField("<size=12>視点場</size>", style);
            //
            // var vpGroupComponent = Object.FindObjectOfType<LandscapeViewPointGroup>();
            // if (vpGroupComponent == null || vpGroupComponent.transform.childCount == 0)
            // {
            //     EditorGUILayout.HelpBox("視点場を作成してください", MessageType.Error);
            //     return false;
            // }
            // vpgroup = vpGroupComponent.gameObject;
            //
            //
            // string[] options = new string[vpgroup.transform.childCount];
            // for (int i = 0; i < vpgroup.transform.childCount; i++)
            // {
            //     LandscapeViewPoint vp = vpgroup.transform.GetChild(i).GetComponent<LandscapeViewPoint>();
            //     options[i] = vp.Name;
            // }
            // selectIndex = EditorGUILayout.Popup(selectIndex, options);
            // EditorGUILayout.Space();
            // EditorGUILayout.LabelField("<size=12>眺望対象</size>", style);
            // GUI.color = selectingTarget == false
            //     ? Color.white
            //     : Color.green;
            // if (GUILayout.Button("眺望対象の選択"))
            // {
            //     sceneView.Focus();
            //     selectingTarget = true;
            // }
            
            return isGuiChanged;
        }
        
        
    }
    #endif
}