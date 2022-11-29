using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using EGIS.ShapeFileLib;

namespace LandscapeDesignTool
{
    public static class LDTTools
    {
        public static string MaterialName = "RegurationAreaMaterial";
        public static Material MakeMaterial(Color col)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // General Transparent Material Settings
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_ZWrite", 0.0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.renderQueue += material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;
            material.SetShaderPassEnabled("ShadowCaster", false);
            material.SetColor("_BaseColor", col);
            material.name = MaterialName;

            return material;
        }

        public static void WriteShapeFile(string filename, string descript, List<List<Vector2>> vertexlist)
        {
            string path = Application.dataPath + "/plugins/LandscapeDesignTool/ShapeFiles/" + filename;

            int nblock = vertexlist.Count;

            Debug.Log("WriteShapeFile "+ filename);
            DbfFieldDesc[] fields = new DbfFieldDesc[]
            {
                new DbfFieldDesc { FieldName="ID", FieldType = DbfFieldType.Character, FieldLength = 14, RecordOffset = 0}
            };


            ShapeFileWriter sfw = ShapeFileWriter.CreateWriter(Application.dataPath + "/plugins/LandscapeDesignTool/ShapeFiles/", filename, ShapeType.Polygon, fields);


            for (int i = 0; i < nblock; i++)
            {
                List<Vector2> vlist = vertexlist[i];

                PointD[] vertex = new PointD[vlist.Count];

                int n = 0;
                Debug.Log("nvertex " + vlist.Count);
                foreach (var v in vlist)
                {
                    vertex[n++] = new PointD(v.x, v.y);
                }


                string[] fielddata = new string[1];
                fielddata[0] = descript+i.ToString();
                sfw.AddRecord(vertex, vertex.Length, fielddata);
            }

            sfw.Close();
        }
    }

#if UNITY_EDITOR
    public class SelectColorPopup : EditorWindow
    {
        public delegate void ColorChangeDelegate(Color col);
        public static ColorChangeDelegate colorChange;
        public delegate void ColorRemoveDelegate();
        public static ColorRemoveDelegate colorRemove;
        static Color _col;
        public static void Init(Color ccol, ColorChangeDelegate dg, ColorRemoveDelegate rm)
        {

            colorChange = dg;
            colorRemove = rm;
            _col = ccol;
            SelectColorPopup window = ScriptableObject.CreateInstance<SelectColorPopup>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 350);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("設定する色を選択", EditorStyles.wordWrappedLabel);
            GUILayout.Space(70);

            _col = EditorGUILayout.ColorField("色の設定", _col);
            GUILayout.Space(10);
            if (GUILayout.Button("色を設定"))
            {
                colorChange.Invoke(_col);
                this.Close();
            }
            if (GUILayout.Button("色を削除"))
            {
                colorRemove.Invoke();
                this.Close();
            }
            if (GUILayout.Button("キャンセル"))
            {
                this.Close();
            }
        }
    }
#endif


}
