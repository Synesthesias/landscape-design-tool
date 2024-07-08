using Landscape2.Runtime.Common;
using UnityEditor;
using UnityEngine;

namespace Landscape2.Runtime.LineOfSight
{
    // 前作での名称は LineOfSightGui
    public class LineOfSightManipulator
    {
        int selectIndex = 0;
        bool selectingTarget = false;
        GameObject vpgroup;
        private Vector3 _prevPos;
        private const string ObjNameLineOfSight = "LineOfSight";
        private const string ObjNameCoveringMesh = "CoveringMesh";
        private static Material MatCompleteTransparent;

        public LineOfSightManipulator(LineOfSight target)
        {
            _prevPos = target.transform.position;
            if (MatCompleteTransparent == null)
            {
                MatCompleteTransparent = Resources.Load<Material>("MaterialCompleteTransparent");
            }
        }
        

        // public enum SurfaceType
        // {
        //     Opaque,
        //     Transparent
        // }


        #if UNITY_EDITOR
        public void OnSceneGUI(LineOfSight target)
        {
            var trans = target.transform;

            bool posStartChanged = false;
            bool posEndChanged = false;
            
            // 視線の始点ハンドルを描画します。
            // ゲームオブジェクトが選択されていて、なおかつEditorのPivotModeがPivotのときは、Unity標準の矢印ハンドルで始点を移動するので、ここでは始点ハンドルは描画しません。
            // 選択されていないときは、始点の矢印ハンドルを描画します。
            // どちらにしても、 transform.position の差分を見て移動があったかどうかを監視します。
            Handles.Label(trans.position, "視線 始点");
            if ((!Selection.Contains(target.gameObject)) || Tools.pivotMode != PivotMode.Pivot )
            {
                trans.position = Handles.PositionHandle(trans.position, Quaternion.identity);
            }
            if ((trans.position - _prevPos).magnitude > 0.0001)
            {
                posStartChanged = true;
            }

            // 視線の終点ハンドルを描画します。
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                Handles.Label(target.EndPos, "視線 終点");
                target.EndPos = Handles.PositionHandle(target.EndPos, Quaternion.identity);
                if (check.changed)
                {
                    posEndChanged = true;
                }
            }

            // 移動があったら視線を生成しなおします。
            if (posStartChanged || posEndChanged)
            {
                var pos = trans.position;
                target.StartPos = pos;
                CreateOrUpdateLineOfSight(target);
                _prevPos = pos;
            }

            if(!selectingTarget) return;

            // var ev = Event.current;

            // RaycastHit hit;
            // if (ev.type == EventType.MouseDown)
            // {
            //
            //     Transform origin = vpgroup.transform.GetChild(selectIndex);
            //     Vector3 originPoint = origin.position;
            //     Vector3 mousePosition = Event.current.mousePosition;
            //
            //
            //     Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            //     if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            //     {
            //         Debug.Log(hit.collider.name);
            //         Debug.Log(hit.collider.bounds.center);
            //
            //         Vector3 targetPoint = hit.point;
            //
            //         selectingTarget = false;
            //         target.EndPos = targetPoint;
            //         CreateOrUpdateLineOfSight(target);
            //         
            //     }
            //     ev.Use();
            // }
            
        }
        #endif

        /// <summary>
        /// 視線を表示するためのゲームオブジェクトを生成します。
        /// すでにあれば、生成の代わりに更新します。
        /// どこに線を出すかは、引数 <paramref name="targetLineOfSight"/> の position, endPos, lineInterval が利用されます。
        /// </summary>
        public void CreateOrUpdateLineOfSight(LineOfSight targetLineOfSight)
        {
            var originPoint = targetLineOfSight.StartPos;
            var targetPoint = targetLineOfSight.EndPos;
            CreateCoveringMesh(targetLineOfSight, originPoint, targetPoint);
            CreateLineOfSight(targetLineOfSight, originPoint, targetPoint);
        }

        /// <summary>
        /// 選択判定のためのメッシュを生成します。
        /// </summary>
        private void CreateCoveringMesh(LineOfSight target, Vector3 originPoint, Vector3 targetPoint)
        {
            // 以前の CoveringMesh があれば削除します。
            var trans = target.transform;
            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                if (child.name == ObjNameCoveringMesh)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            var wSize = target.ScreenWidth;
            var hSize = target.ScreenHeight;
            float length = (targetPoint - originPoint).magnitude;
            Vector3[] vertex = new Vector3[6];
            vertex[0] = new Vector3(0, 0, 0);
            vertex[1] = new Vector3(-wSize / 2.0f, -hSize / 2.0f, length);
            vertex[2] = new Vector3(-wSize / 2.0f, hSize / 2.0f, length);
            vertex[3] = new Vector3(wSize / 2.0f, hSize / 2.0f, length);
            vertex[4] = new Vector3(wSize / 2.0f, -hSize / 2.0f, length);
            vertex[5] = new Vector3(0, 0, length);

            int[] idx = {
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 1,
                5, 2, 1,
                5, 3, 2,
                5, 4, 3,
                5, 1, 4 };

            GameObject go = new GameObject(ObjNameCoveringMesh);
            // go.layer = LayerMask.NameToLayer("RegulationArea");

            var mf = go.GetComponent<MeshFilter>();
            if (mf == null)
                mf = go.AddComponent<MeshFilter>();

            var mesh = new Mesh();
            mesh.vertices = vertex;
            mesh.triangles = idx;

            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = go.AddComponent<MeshRenderer>();

            // Editorからクリックで選択できるようにMeshRendererは表示しておきたいが、
            // 見た目には影響してほしくないので完全に透明なマテリアルを使う。
            Material material = MatCompleteTransparent;
            mr.sharedMaterial = material;
            mf.mesh = mesh;

            go.transform.position = originPoint;
            go.transform.LookAt(targetPoint, Vector3.up);
            go.transform.parent = (target).transform;
        }

        private static void ClearLineOfSight(LineOfSight target)
        {
            var root = (target).transform;
            for (int i = 0; i < root.childCount; ++i)
            {
                var trans = root.GetChild(i);
                string childName = trans.name;
                if (childName == ObjNameLineOfSight)
                {
                    Object.DestroyImmediate(trans.gameObject);
                }
            }
        }

        float CreateLineOfSight(LineOfSight target, Vector3 origin, Vector3 destination)
        {
            ClearLineOfSight(target);

            var obj = new GameObject(ObjNameLineOfSight);
            obj.transform.parent = ((LineOfSight)target).transform;

            float result = -1;
            float interval = target.LineInterval;
            int divx = (int)(target.ScreenWidth / interval);
            int divy = (int)(target.ScreenHeight / interval);
            var diff = destination - origin;

            for (int i = 0; i < divx + 1; i++)
            {
                for (int j = 0; j < divy + 1; j++)
                {
                    
                    // EndPosを通り、StartPosからEndPosへのベクトルに垂直な面を想定したものです。視線ターゲットの面です。
                    // その面の中心から視線へのベクトルを求めます。
                    var planeDiff = new Vector2(
                        -target.ScreenWidth / 2.0f + interval * i,
                        -target.ScreenHeight / 2.0f + interval * j);// EndPosから線投射先へのベクトル
                    // 始点から終点のベクトルの角度を求めます。
                    float angle = Vector2.SignedAngle(Vector2.up, new Vector2(diff.x, diff.z));
                    var rotator = Quaternion.AngleAxis(-angle, Vector3.up);
                    // まず、始点から終点のベクトルが真北を向いており、始点が(0,0,0)にあるケースを計算します。
                    var notRotatedLine =
                        new Vector3(planeDiff.x, planeDiff.y, 0) + new Vector3(0, diff.y, diff.magnitude);
                    // それを角度で回転し、始点を望みの位置に回転することで線を得ます。
                    var d = rotator * notRotatedLine + origin;
                    RaycastHit hit;

                    if (RaycastBuildings(target, origin, d, out hit))
                    {
                        DrawLine(origin, hit.point, obj, target.LineColorValid);
                        DrawLine(hit.point, d, obj, target.LineColorInvalid);
                    }
                    else
                    {
                        DrawLine(origin, d, obj, target.LineColorValid);
                    }
                }
            }

            return result;
        }

        void DrawLine(Vector3 origin, Vector3 distination, GameObject parent, Color col)
        {
            #if UNITY_EDITOR
            Vector3[] point = new Vector3[2];
            point[0] = origin;
            point[1] = distination;

            GameObject go = new GameObject("ViewRegurationAreaByLine");
            // go.layer = LayerMask.NameToLayer("RegulationArea");
            SceneVisibilityManager.instance.DisablePicking(go, true); // 視線オブジェクトの数が多すぎるので、選択できないほうが便利

            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();

            lineRenderer.SetPositions(point);
            lineRenderer.positionCount = point.Length;
            lineRenderer.startWidth = 1.0f;
            lineRenderer.endWidth = 1.0f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            lineRenderer.startColor = col;
            lineRenderer.endColor = col;

            go.transform.parent = parent.transform;
            #endif
        }

        bool RaycastBuildings(LineOfSight target, Vector3 origin, Vector3 destination, out RaycastHit hitInfo)
        {
            bool result = false;

            hitInfo = new RaycastHit();

            Vector3 direction = (destination - origin).normalized;

            RaycastHit[] hits;
            hits = Physics.RaycastAll(origin, direction, (destination - origin).magnitude);

            float minDistance = float.MaxValue;
            if (hits.Length <= 0)
                return result;

            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.name == target.name)
                    continue;

                int layerIgnoreRaycast = LayerMask.NameToLayer("RegulationArea");

                if (hit.collider.gameObject.layer == layerIgnoreRaycast)
                    continue;

                result = true;

                if (hit.distance >= minDistance)
                    continue;

                hitInfo = hit;
                minDistance = hit.distance;
            }

            return result;
        }
    }
}