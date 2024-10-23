using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Landscape2.Runtime.UiCommon;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画の頂点を編集するクラス
    /// </summary>
    public class AreaPlanningEdit
    {
        private readonly AreaEditManager areaEditManager;
        private readonly DisplayPinLine displayPinLine;
        private readonly LandscapePlanMeshModifier landscapePlanMeshModifier;
        private readonly TessellatedMeshCreator tessellatedMeshCreator;
        private readonly WallGenerator wallGenerator;
        private List<Vector3> vertices = new List<Vector3>();
        private bool isVertexEditing = false;
        private GameObject editingPin;
        private GameObject editingLine;


        public AreaPlanningEdit(AreaEditManager areaEditManager,DisplayPinLine displayPinLine)
        {
            this.areaEditManager = areaEditManager;
            this.displayPinLine = displayPinLine;
            landscapePlanMeshModifier = new LandscapePlanMeshModifier();
            tessellatedMeshCreator = new TessellatedMeshCreator();
            wallGenerator = new WallGenerator();
        }

        /// <summary>
        /// 変更内容を確定・保持するメソッド
        /// </summary>
        public void ConfirmEditData()
        {
            if (!isVertexEditing) return;

            // 頂点の編集を適用
            EditVertexIfClicked();
            // メッシュを生成
            CreateMeshByVertice();
        }

        /// <summary>
        /// 編集した頂点データを基にメッシュを生成するメソッド
        /// </summary>
        private void CreateMeshByVertice()
        {
            if (!isVertexEditing) return;
            List<List<Vector3>> listOfVertices = new List<List<Vector3>>();
            listOfVertices.Add(new List<Vector3>(vertices));

            // テッセレーション処理を行ったメッシュを生成
            GameObject gisObject = GameObject.Find("Area_" + areaEditManager.GetAreaID());
            if (gisObject == null) return;
            
            MeshFilter gisObjMeshFilter = gisObject.GetComponent<MeshFilter>();
            tessellatedMeshCreator.CreateTessellatedMesh(listOfVertices, gisObjMeshFilter, 30, 40);
            Mesh mesh = gisObjMeshFilter.sharedMesh;
            AreaProperty areaProperty = AreasDataComponent.GetProperty(areaEditManager.GetAreaID());

            // Meshを変形
            if (!landscapePlanMeshModifier.TryModifyMeshToTargetHeight(mesh, areaProperty.LimitHeight, gisObject.transform.position))
            {
                Debug.LogError($"{gisObject.name} is out of range of the loaded map");
                return;
            }

            // 区画のメッシュから下向きに壁を生成
            GameObject[] walls = wallGenerator.GenerateWall(mesh, (float)areaEditManager.GetMaxHeight(), Vector3.down, areaEditManager.GetWallMaterial());
            for (int j = 0; j < walls.Length; j++)
            {
                GameObject wallObject = GameObject.Find($"AreaWall_{areaEditManager.GetAreaID()}_{j}");
                // 存在する壁オブジェクトを削除
                if (wallObject != null) GameObject.Destroy(wallObject);

                walls[j].transform.SetParent(gisObject.transform);
                walls[j].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                walls[j].name = $"AreaWall_{areaEditManager.GetAreaID()}_{j}";


                areaProperty.SetLocalPosition(new Vector3(
                    areaProperty.Transform.localPosition.x,
                    areaProperty.LimitHeight,
                    areaProperty.Transform.localPosition.z
                    ));
            }           
        }

        /// <summary>
        /// エリアの頂点座標リストを取得し，ピンとラインを表示するメソッド
        /// </summary>
        public void CreatePinline()
        {
            int count = areaEditManager.GetPointCount();
            if(count < 3) return;
            Vector3 startVec = Vector3.zero;
            Vector3 endVec = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                Vector3 vertex = areaEditManager.GetPointData(i);
                if (vertex == null) return;

                vertices.Add(vertex);

                // ラインを描画
                if (i == 0)
                {
                    endVec = landscapePlanMeshModifier.SearchGroundPoint(vertex) + new Vector3(0, 5.0f, 0);
                }
                else
                {
                    startVec = endVec;
                    endVec = landscapePlanMeshModifier.SearchGroundPoint(vertex) + new Vector3(0, 5.0f, 0);
                    displayPinLine.DrawLine(startVec, endVec, vertices.Count - 2);
                }
                displayPinLine.CreatePin(endVec, vertices.Count - 1);
            }
            // エリアを閉じる
            startVec = landscapePlanMeshModifier.SearchGroundPoint(vertices[0]) + new Vector3(0, 5.0f, 0);
            displayPinLine.DrawLine(endVec, startVec, vertices.Count - 1);
        }

        /// <summary>
        /// ピンをクリックしたかどうかを判定するメソッド
        /// </summary>
        public bool IsClickPin()
        {
            RaycastHit[] hits;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, Mathf.Infinity);
            if (hits == null || hits.Length == 0)
                return false;
            // ピンをクリックした場合
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.name.Contains("Pin"))
                {
                    editingPin = hits[i].collider.gameObject;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///ラインをクリックしたかどうかを判定するメソッド
        /// </summary>
        public bool IsClickLine()
        {
            RaycastHit[] hits;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, Mathf.Infinity);
            if (hits == null || hits.Length == 0)
                return false;
            // ラインをクリックした場合
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.name.Contains("Line"))
                {
                    editingLine = hits[i].collider.gameObject;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///ラインの中点に新しく頂点を追加するメソッド
        /// </summary>
        public void AddVertexToLine()
        {
            int lineIndex = displayPinLine.FindLineIndex(editingLine);
            if (lineIndex == -1) return;

            //// y座標は前後の頂点の中点の座標にする           
            Vector3 previousVec = vertices[lineIndex];
            Vector3 nextVec;
            if (lineIndex == vertices.Count - 1)
            {
                nextVec = vertices[0];
            }
            else
            { 
                nextVec = vertices[lineIndex + 1];           
            }
            Vector3 newVec = (previousVec + nextVec) / 2;
            vertices.Insert(lineIndex + 1, newVec);

            // ピンとラインを挿入
            Vector3 newPinVec = landscapePlanMeshModifier.SearchGroundPoint(newVec) + new Vector3(0, 5.0f, 0);
            displayPinLine.InsertPinLine(newPinVec, lineIndex);
        }

        /// <summary>
        /// ピンをドラッグしたときの処理
        /// </summary>
        public void OnDragPin()
        {
            if (editingPin == null)return;

            Vector3 objPos = Camera.main.WorldToScreenPoint(editingPin.transform.position);
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, objPos.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            editingPin.transform.position = new Vector3(worldPos.x, editingPin.transform.position.y, worldPos.z);
            // ラインの頂点を移動させる
            displayPinLine.MoveLineVertex(editingPin, editingPin.transform.position);
            // 頂点データを更新
            int index = displayPinLine.FindPinIndex(editingPin);
            if (index == -1) return;
            vertices[index] = new Vector3(editingPin.transform.position.x, vertices[index].y, editingPin.transform.position.z);
            
            isVertexEditing = true;
        }

        /// <summary>
        /// ピンを離したときの処理
        /// </summary>
        public void OnReleasePin()
        {
            editingPin = null;
            editingLine = null;
        }

        /// <summary>
        /// エリアの頂点の編集を適用させる処理
        /// </summary>
        public void EditVertexIfClicked()
        {
            areaEditManager.EditPointData(new List<Vector3>(vertices));
        }

        /// <summary>
        /// エリアの頂点の編集をクリアする処理
        /// </summary>
        public void ClearVertexEdit()
        {
            vertices.Clear();
            displayPinLine.ClearPins();
            displayPinLine.ClearLines();
            isVertexEditing = false;
        }

        /// <summary>
        /// 2つの線分が交差しているかを判定するヘルパーメソッド (Y軸無視)
        /// </summary>
        private bool DoLinesIntersect2D(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            // 線分p1-p2と線分p3-p4がXY平面で交差しているか判定する

            // p1, p2の線分がp3, p4の線分をまたいでいるか判定
            float d1 = Direction2D(p3, p4, p1);
            float d2 = Direction2D(p3, p4, p2);
            float d3 = Direction2D(p1, p2, p3);
            float d4 = Direction2D(p1, p2, p4);

            // 互いに異なる方向に向かっている場合は交差している
            if (d1 * d2 < 0 && d3 * d4 < 0)
                return true;

            // 特殊ケース: 線分が同じ直線上に存在する場合（共線）
            if (d1 == 0 && OnSegment2D(p3, p4, p1)) return true;
            if (d2 == 0 && OnSegment2D(p3, p4, p2)) return true;
            if (d3 == 0 && OnSegment2D(p1, p2, p3)) return true;
            if (d4 == 0 && OnSegment2D(p1, p2, p4)) return true;

            return false;
        }

        /// <summary>
        /// 方向を判定するヘルパーメソッド (Y軸無視)
        /// </summary>
        private float Direction2D(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // p1-p2ベクトルとp1-p3ベクトルの外積で方向を判定（Y軸を無視）
            return (p3.x - p1.x) * (p2.z - p1.z) - (p2.x - p1.x) * (p3.z - p1.z);
        }

        /// <summary>
        /// 点pが線分p1-p2上にあるかを判定するヘルパーメソッド (Y軸無視)
        /// </summary>
        private bool OnSegment2D(Vector3 p1, Vector3 p2, Vector3 p)
        {
            // Y軸を無視してXとZのみで判定
            return Mathf.Min(p1.x, p2.x) <= p.x && p.x <= Mathf.Max(p1.x, p2.x) &&
                   Mathf.Min(p1.z, p2.z) <= p.z && p.z <= Mathf.Max(p1.z, p2.z);
        }

        /// <summary>
        /// Vector3のリストがXY平面上でクロスしているかを判定するメソッド (Y軸無視)
        /// </summary>
        public bool AreVerticesCrossing2D(List<Vector3> vertices)
        {
            // 頂点が少なくとも3つ必要（線分が2つ以上必要）
            if (vertices.Count < 4) return false;

            // 頂点リスト内の全線分ペアの交差を判定 (Y軸無視)
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                for (int j = i + 2; j < vertices.Count - 1; j++)
                {
                    // 隣接する線分同士は交差しない（同じ頂点を共有するため）
                    if (i == 0 && j == vertices.Count - 1) continue; // 最後の頂点と最初の頂点は無視

                    if (DoLinesIntersect2D(vertices[i], vertices[i + 1], vertices[j], vertices[j + 1]))
                    {
                        return true; // 交差している
                    }
                }
            }
            return false; // 交差なし
        }
    }
}
