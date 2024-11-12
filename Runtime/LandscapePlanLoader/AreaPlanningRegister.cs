using Landscape2.Runtime.UiCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画の頂点を作成するクラス
    /// </summary>
    public class AreaPlanningRegister
    {
        private LandscapePlanLoadManager landscapePlanLoadManager;
        private DisplayPinLine displayPinLine;
        private bool isClosed = false;
        private List<Vector3> vertices = new List<Vector3>();

        public AreaPlanningRegister(DisplayPinLine displayPinLine)
        {
            this.displayPinLine = displayPinLine;
            landscapePlanLoadManager = new LandscapePlanLoadManager();
        }

        /// <summary>
        /// エリア作成可能かを判定するメソッド
        /// </summary>
        public bool IsCreateArea()
        {
            if (vertices.Count < 3)
            {
                // LogWarningを表示
                Debug.LogWarning("頂点数が3未満です。");
                return false;
            }

            if (AreVerticesCrossing2D(vertices))
            {
                // LogWarningを表示
                Debug.LogWarning("頂点が交差しています。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// クリック時にピンとライン生成を行うメソッド
        /// </summary>
        public void AddVertexIfClicked()
        {
            if (!isClosed)
            { 
                RaycastHit[] hits;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hits = Physics.RaycastAll(ray, Mathf.Infinity);
                if (hits == null || hits.Length == 0)
                    return;

                if (vertices.Count > 2)
                {
                    // 最初に生成したピンの場合はエリアを閉じる
                    if (displayPinLine.IsClickFirstPin(hits))
                    {
                        var startVec = vertices[vertices.Count - 1] + new Vector3(0, 5.0f, 0);
                        displayPinLine.DrawLine(startVec, vertices[0] + new Vector3(0, 5.0f, 0), vertices.Count - 1);
                        isClosed = true;
                        return;
                    }
                }

                for (int i = 0; i < hits.Length; i++)
                {                  
                    if (hits[i].collider.gameObject.name.Contains("dem_"))
                    {
                        vertices.Add(hits[i].point);
                        var newVec = hits[i].point + new Vector3(0, 5.0f, 0);
                        // ピンを生成
                        displayPinLine.CreatePin(newVec, vertices.Count - 1);
                        // ラインを生成
                        if (vertices.Count > 1)
                        {
                            var startVec = vertices[vertices.Count - 2] + new Vector3(0, 5.0f, 0);
                            displayPinLine.DrawLine(startVec, newVec, vertices.Count - 2);

                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 景観区画データを作成するメソッド
        /// </summary>
        public void CreateAreaData(string name,float height,float wallMaxHeight,Color color)
        {
            int id = AreasDataComponent.GetPropertyCount();
            List<List<Vector3>> listOfVertices = new List<List<Vector3>>();
            // 頂点データが反時計回りの場合は反転
            if (!IsClockwise())
            {
                vertices.Reverse();
            }
            listOfVertices.Add(new List<Vector3>(vertices));

            // 新規景観区画データを作成
            PlanAreaSaveData newSaveData = new PlanAreaSaveData(
                id,
                name,
                height,
                10.0f,
                color,
                wallMaxHeight,
                listOfVertices,
                false
                );
            List<PlanAreaSaveData>listOfSaveData = new List<PlanAreaSaveData>
            {
                newSaveData
            };
            // 作成した景観区画データ基に景観データをロード
            landscapePlanLoadManager.LoadFromSaveData(listOfSaveData);
        }

        /// <summary>
        /// エリアが閉じられたかどうかを判定するメソッド
        /// </summary>
        public bool IsClosed()
        {
            return isClosed;
        }

        /// <summary>
        /// エリアの頂点の編集をクリアする処理
        /// </summary>
        public void ClearVertexEdit()
        {
            vertices.Clear();
            displayPinLine.ClearPins();
            displayPinLine.ClearLines();
            isClosed = false;
        }

        /// <summary>
        /// 頂点が時計回りかどうかを判定するメソッド
        /// </summary>
        private bool IsClockwise()
        {
            if (vertices.Count < 3)
            {
                Debug.LogWarning("頂点数が3未満です。");
            }
            float sum = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vec1 = vertices[i];
                Vector3 vec2 = vertices[(i + 1) % vertices.Count];
                sum += (vec2.x - vec1.x) * (vec2.z + vec1.z);
            }
            return sum > 0;
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
