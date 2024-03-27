using UnityEditor;

namespace Landscape2.Editor
{
    /// <summary>
    /// 景観ツールのEditorWindowのエントリーポイントです。
    /// </summary>
    public class LandscapeWindow : EditorWindow
    {
        [MenuItem("PLATEAU/Landscape 2")]
        public static void Open()
        {
            var window = GetWindow<LandscapeWindow>("Landscape 2");
            window.Show();
        }
    }
}
