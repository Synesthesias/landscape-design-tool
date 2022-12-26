using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor
{
    [CustomEditor(typeof(LandscapeViewPoint))]
    public class LandScapeViewPointEditor : UnityEditor.Editor
    {
        public static LandScapeViewPointEditor Active;

        private SerializedProperty fovProperty;

        public LandscapeViewPoint Target => target as LandscapeViewPoint;

        private void OnEnable()
        {
            fovProperty = serializedObject.FindProperty("fov");
        }

        public override void OnInspectorGUI()
        {
            Active = this;

            Target.gameObject.name = EditorGUILayout.TextField("éãì_èÍñº", Target.gameObject.name);

            fovProperty.floatValue = EditorGUILayout.FloatField("éãñÏäp", fovProperty.floatValue);
            Target.Camera.fieldOfView = fovProperty.floatValue;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
