using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor
{
    [CustomEditor(typeof(ViewRegulation))]
    public class ViewRegulationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
        }

        private void OnSceneGUI()
        {
            Handles.PositionHandle(new Vector3(1709, 74, -389), Quaternion.identity);
            Handles.PositionHandle(new Vector3(818, 121, 783), Quaternion.identity);
            Handles.PositionHandle(new Vector3(975, 121, 856), Quaternion.identity);
        }
    }
}
