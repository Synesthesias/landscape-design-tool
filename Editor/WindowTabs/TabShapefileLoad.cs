using UnityEditor;
using UnityEngine;

namespace LandscapeDesignTool.Editor.WindowTabs
{
    /// <summary>
    /// Shapefile読込のタブを描画します。
    /// </summary>
    public class TabShapefileLoad
    {
        private ShapeFileEditorHelper _shapeFileEditorHelper;
        public void Draw(GUIStyle labelStyle)
        {
            _shapeFileEditorHelper = (ShapeFileEditorHelper)EditorGUILayout.ObjectField("ShapefileHelper選択",
                _shapeFileEditorHelper,
                typeof(ShapeFileEditorHelper),
                true);
            if (_shapeFileEditorHelper != null)
            {
                _shapeFileEditorHelper.DrawGui();
            }
        }
    }
}