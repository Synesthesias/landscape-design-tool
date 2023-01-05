using UnityEditor;

namespace LandscapeDesignTool.Editor
{
    [CustomEditor(typeof(LandscapeViewPoint))]
    public class LandScapeViewPointEditor : UnityEditor.Editor
    {
        public static LandScapeViewPointEditor Active;

        public LandscapeViewPoint Target => target as LandscapeViewPoint;
        
        public override void OnInspectorGUI()
        {
            Active = this;

            Target.gameObject.name = EditorGUILayout.TextField("éãì_èÍñº", Target.gameObject.name);

            Target.Fov = EditorGUILayout.FloatField("éãñÏäp", Target.Fov);
            Target.Camera.fieldOfView = Target.Fov;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
