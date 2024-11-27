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
        private Vector3 lastPinPosition;


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
            if (!isVertexEditing)
            {
                Debug.LogWarning("Vertex is not edited");
                return;
            }
            List<List<Vector3>> listOfVertices = new List<List<Vector3>>
            {
                new List<Vector3>(vertices)
            };

            // テッセレーション処理を行ったメッシュを生成
            GameObject gisObject = GameObject.Find("Area_" + areaEditManager.GetAreaID());
            if (gisObject == null)
            {
                Debug.LogError("GIS object is not found");
                return;
            } 
            
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
            if (count < 3)
            {
                Debug.LogWarning("The number of vertices is less than 3");
                return;
            };
            Vector3 startVec = Vector3.zero;
            Vector3 endVec = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                Vector3 vertex = areaEditManager.GetPointData(i);
                if (vertex == null)
                {
                    Debug.LogWarning("Vertex is null");
                    return;
                }

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
                    lastPinPosition = editingPin.transform.position;
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
            if (lineIndex == -1)
            { 
                Debug.LogWarning("Line is not found");
                return;
            };

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

            Vector3 pinPos = Camera.main.WorldToScreenPoint(editingPin.transform.position);
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pinPos.z);
            // 地面の高さを取得
            Vector3 groundPos = landscapePlanMeshModifier.SearchGroundPoint(editingPin.transform.position);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            editingPin.transform.position = new Vector3(worldPos.x, groundPos.y, worldPos.z) + new Vector3(0, 5.0f, 0);
            // ラインの頂点を移動させる
            displayPinLine.MoveLineVertex(editingPin, editingPin.transform.position);
            // 頂点データを更新
            int index = displayPinLine.FindPinIndex(editingPin);
            if (index == -1) return;
            vertices[index] = new Vector3(editingPin.transform.position.x, groundPos.y, editingPin.transform.position.z);
            
            isVertexEditing = true;
        }

        /// <summary>
        /// ピンを離したときの処理
        /// </summary>
        public void OnReleasePin()
        {
            if(editingPin == null)return;

            // ラインが交差しているかを判定
            if (displayPinLine.IsIntersectedByPin(editingPin))
            {
                Debug.LogWarning("Vertices are crossing");
                // 頂点，ピン，ラインを元の位置に戻す
                editingPin.transform.position = lastPinPosition;
                vertices[displayPinLine.FindPinIndex(editingPin)] = lastPinPosition;
                displayPinLine.MoveLineVertex(editingPin, lastPinPosition);
            }
            editingPin = null;
            editingLine = null;           
        }

        /// <summary>
        /// 頂点を削除するメソッド
        /// </summary>
        public void DeleteVertex()
        {
            if (editingPin == null)
            {
                Debug.LogWarning("Pin is not selected");
                return;
            }
            if (vertices.Count < 4)
            {
                Debug.LogWarning("The number of vertices is less than 4");
                return;
            }

            int index = displayPinLine.FindPinIndex(editingPin);
            if (index == -1)
            { 
                Debug.LogWarning("Pin is not found");
                return;
            };

            // ピンとラインを削除
            displayPinLine.RemovePinLine(index);
            vertices.RemoveAt(index);
            isVertexEditing = true;
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
    }
}
