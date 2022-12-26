using EGIS.ShapeFileLib;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LandscapeDesignTool
{
    public static class LDTTools
    {
        public enum AreaType
        {
            CIRCLE_REGURATION,
            POLYCGON_REGURATION
        }

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

        public static void WriteShapeFile(string exportFilePath, string areatype, string[] type, Color[] col, float[] height, Vector2[,] specpoint, List<List<Vector2>> vertexlist/*,  List<int> instanceList*/)
        {

            int nblock = vertexlist.Count;

            Debug.Log("WriteShapeFile to : " + exportFilePath);
            DbfFieldDesc[] fields = new DbfFieldDesc[7];
            fields[0] = new DbfFieldDesc { FieldName = "ID", FieldType = DbfFieldType.Character, FieldLength = 14, RecordOffset = 0 };
            fields[1] = new DbfFieldDesc { FieldName = "AREATYPE", FieldType = DbfFieldType.Character, FieldLength = 14, RecordOffset = 0 };
            fields[2] = new DbfFieldDesc { FieldName = "TYPE", FieldType = DbfFieldType.Character, FieldLength = 14, RecordOffset = 0 };
            fields[3] = new DbfFieldDesc { FieldName = "HEIGHT", FieldType = DbfFieldType.Character, FieldLength = 14, RecordOffset = 0 };
            fields[4] = new DbfFieldDesc { FieldName = "COLOR", FieldType = DbfFieldType.Character, FieldLength = 128, RecordOffset = 0 };
            fields[5] = new DbfFieldDesc { FieldName = "POINT1", FieldType = DbfFieldType.Character, FieldLength = 128, RecordOffset = 0 };
            fields[6] = new DbfFieldDesc { FieldName = "POINT2", FieldType = DbfFieldType.Character, FieldLength = 128, RecordOffset = 0 };

            string exportBaseDirPath = Directory.GetParent(exportFilePath)?.FullName;
            if (string.IsNullOrEmpty(exportBaseDirPath))
            {
                Debug.LogError($"Export path is invalid. path = {exportFilePath}");
                return;
            }
            ShapeFileWriter sfw = ShapeFileWriter.CreateWriter(exportBaseDirPath, Path.GetFileNameWithoutExtension(exportFilePath), ShapeType.Polygon, fields);

            for (int i = 0; i < nblock; i++)
            {
                List<Vector2> vlist = vertexlist[i];

                PointD[] vertex = new PointD[vlist.Count];

                int n = 0;

                foreach (var v in vlist)
                {
                    vertex[n++] = new PointD(v.x, v.y);
                    Debug.Log(vertex[n - 1].ToString());
                }

                string[] fielddata = new string[7];
                fielddata[0] = i.ToString();
                fielddata[1] = areatype;
                fielddata[2] = type[i];
                fielddata[3] = height[i].ToString();
                fielddata[4] = col[i].r.ToString() + "," + col[i].g.ToString() + "," + col[i].b.ToString() + "," + col[i].a.ToString();
                Debug.Log(fielddata[4]);
                fielddata[5] = specpoint[i, 0].x.ToString() + ", " + specpoint[i, 0].y.ToString();
                fielddata[6] = specpoint[i, 1].x.ToString() + ", " + specpoint[i, 1].y.ToString();

                sfw.AddRecord(vertex, vertex.Length, fielddata);
            }

            sfw.Close();
        }

        static string[] layerName = { "RegulationArea" };
        static int[] layerId = { 30 };
        public static void CheckLayers()
        {

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            //layer情報を取得
            var layersProp = tagManager.FindProperty("layers");
            var index = 0;
            foreach (var layerId in layerId)
            {
                if (layersProp.arraySize > layerId)
                {
                    var sp = layersProp.GetArrayElementAtIndex(layerId);
                    if (sp != null && sp.stringValue != layerName[index])
                    {
                        sp.stringValue = layerName[index];
                        Debug.Log("Adding layer " + layerName[index]);
                    }
                }

                index++;
            }

            tagManager.ApplyModifiedProperties();
            tagManager.Update();

        }

        public static void CheckTag(string tagname)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            //layer情報を取得
            var tags = tagManager.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tagname)
                {
                    return;
                }
            }

            int index = tags.arraySize;
            tags.InsertArrayElementAtIndex(index);
            tags.GetArrayElementAtIndex(index).stringValue = tagname;
            tagManager.ApplyModifiedProperties();
            tagManager.Update();
        }

        public static string GetNumberWithTag(string tagname, string title)
        {
            int rval = -1;

            int maxvalule = 0;
            CheckTag(tagname);
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tagname);
            foreach (GameObject obj in objects)
            {
                string nm = obj.name;
                string[] s1 = nm.Split('-');

                if (s1.Length < 2)
                    continue;

                if (!int.TryParse(s1[1], out int n))
                    continue;

                maxvalule = Mathf.Max(maxvalule, n);
            }
            rval = maxvalule + 1;

            return title + "-" + rval.ToString();
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
        static Color orgColor;
        static bool hasMaterial = false;
        public static void Init(Color ccol, ColorChangeDelegate dg, ColorRemoveDelegate rm, bool f)
        {

            colorChange = dg;
            colorRemove = rm;
            orgColor = ccol;
            _col = ccol;
            hasMaterial = f;
            SelectColorPopup window = ScriptableObject.CreateInstance<SelectColorPopup>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 350);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("設定する色を選択", EditorStyles.wordWrappedLabel);
            GUILayout.Space(70);

            colorChange.Invoke(_col);

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
                if (!hasMaterial)
                {
                    colorRemove.Invoke();

                }
                else
                {
                    colorChange.Invoke(orgColor);

                }
                this.Close();
            }
        }
    }
#endif


}
