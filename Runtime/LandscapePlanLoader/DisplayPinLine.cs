using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画作成・編集画面の頂点編集を行うクラス
    /// </summary>
    public class DisplayPinLine
    {
        private GameObject pin; // ピンのプレハブ
        private GameObject line;   // ラインのプレハブ
        private List<GameObject> pinList = new List<GameObject>();
        private List<GameObject> lineList = new List<GameObject>();
        private bool isClosed = false;

        private float scaleStep = 1f;       // スクロールごとのスケール変化量
        private float widthStep = 0.2f;       // スクロールごとのスケール変化量
        private float minScale = 10f;        // 最小スケール
        private float minWidth = 2f;        // 最小幅
        private float maxScale = 30f;        // 最大スケール
        private float maxWidth = 6f;        // 最大幅
        private float currentScale = 10f;   // 現在のスケール（初期値）
        private float currentWidth = 2f;   // 現在のライン幅（初期値）
        private float scaleValue = 10f;    // スケール値
        private float widthValue = 2f;   // ライン幅値

        private float lineColliderHeight = 100f; // 交差判定のラインのコライダーの高さ
        private float lineColliderWidth = 2f; // 交差判定のラインのコライダーの幅
        private float cameraHeight = 215.0f; // カメラの高さの標準値

        public DisplayPinLine()
        {
            pin = Resources.Load("PlanAreaPin") as GameObject;
            line = Resources.Load("PlanAreaLine") as GameObject;
            scaleValue = currentScale;
            widthValue = currentWidth;
        }

        /// <summary>
        /// ピンを生成するメソッド
        /// </summary>
        public void CreatePin(Vector3 vec,int index)
        {
            // クリックした位置にピンを生成
            if (pin == null)
            {
                Debug.LogWarning("ピン用のオブジェクトが見つかりません。");
                return;
            }
            var pinObj = GameObject.Instantiate(pin);
            pinObj.transform.localScale = new Vector3(
                currentScale,
                currentScale,
                currentScale
            );
            pinObj.transform.position = vec;
            pinList.Insert(index,pinObj);
        }

        /// <summary>
        /// ピンとラインのサイズの初期化を行うメソッド
        /// </summary>
        public void InitializePinLineSize()
        {
            // カメラの高さの標準値に応じてピンとラインのサイズを初期化
            if (Camera.main != null)
            {
                currentScale = Camera.main.transform.position.y / cameraHeight * 10f;
                currentWidth = Camera.main.transform.position.y / cameraHeight * 2f;
            }
        }

        /// <summary>
        /// Lineを生成するメソッド
        /// </summary>
        public void DrawLine(Vector3 startVec,Vector3 endVec,int index) 
        {
            if (line == null)
            {
                Debug.LogWarning("Line用のオブジェクトが見つかりません。");
                return;
            }
            var lineObj = GameObject.Instantiate(line);
            lineObj.name = "Line" + index;

            // 線を引く
            UpdateLinePositions(lineObj,startVec,endVec);

            lineList.Insert(index, lineObj);
        }

        /// <summary>
        /// PinとLineを挿入するメソッド
        /// </summary>
        public void InsertPinLine(Vector3 newVec,int index)
        {
            // 元のラインを削除
            GameObject.Destroy(lineList[index]);
            lineList.RemoveAt(index);

            // 新しいピンとラインを挿入
            CreatePin(newVec, index + 1);
            DrawLine(pinList[index].transform.position, newVec, index);
            if (index == lineList.Count - 1)
            {
                DrawLine(newVec, pinList[0].transform.position, index + 1);
            }
            else
            {
                DrawLine(newVec, pinList[index + 2].transform.position, index + 1);
            }

        }

        /// <summary>
        /// PinとLineを削除するメソッド
        /// </summary>
        public void RemovePinLine(int index)
        {
            // ピンを削除
            GameObject.Destroy(pinList[index]);
            pinList.RemoveAt(index);

            // ラインの終点を更新
            GameObject.Destroy(lineList[index]);
            lineList.RemoveAt(index);
            if (index == 0)
            {
                GameObject.Destroy(lineList[lineList.Count - 1]);
                lineList.RemoveAt(lineList.Count - 1);
                DrawLine(pinList[pinList.Count - 1].transform.position, pinList[0].transform.position, index);
            }
            else if(index == pinList.Count)
            {
                GameObject.Destroy(lineList[index - 1]);
                lineList.RemoveAt(index - 1);
                DrawLine(pinList[index - 1].transform.position, pinList[0].transform.position, index - 1);
            }
            else
            {
                GameObject.Destroy(lineList[index - 1]);
                lineList.RemoveAt(index - 1);
                DrawLine(pinList[index - 1].transform.position, pinList[index].transform.position, index - 1);
            }
        }

        /// <summary>
        /// Lineの頂点を動かすメソッド
        /// </summary>
        public void MoveLineVertex(GameObject editingPin, Vector3 newVec)
        {
            int index = FindPinIndex(editingPin);
            if(index == -1) return;

            if (index == lineList.Count - 1)
            {
                UpdateLinePositions(lineList[index], newVec, pinList[0].transform.position);
            }
            else 
            {
                UpdateLinePositions(lineList[index], newVec, pinList[index + 1].transform.position);
            }

            // 対象の頂点が終点となるラインの頂点を編集
            if (index == 0)
            {
                UpdateLinePositions(lineList[lineList.Count - 1], pinList[lineList.Count - 1].transform.position, newVec);
            }
            else
            {
                UpdateLinePositions(lineList[index - 1], pinList[index - 1].transform.position, newVec);
            }
        }

        /// <summary>
        /// Lineオブジェクトの頂点を更新し、線とコライダーを設定するメソッド
        /// </summary>
        private void UpdateLinePositions(GameObject lineObj, Vector3 newStartVec, Vector3 newEndVec)
        {
            Vector3 lineVec = newEndVec - newStartVec;
            float dist = lineVec.magnitude; // 新しい線の長さ
            Vector3 lineX = new Vector3(dist, 0f, 0f); // X軸方向に伸びているベクトル

            // LineRendererの更新
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

            if (lineRenderer != null)
            {
                lineRenderer.useWorldSpace = false;
                lineRenderer.positionCount = 2;
                Vector3[] linePositions = new Vector3[] { Vector3.zero, lineX };
                lineRenderer.SetPositions(linePositions);

                // BoxColliderの更新
                BoxCollider col = lineObj.GetComponent<BoxCollider>();
                if (col == null)
                {
                    col = lineObj.AddComponent<BoxCollider>();
                }

                // コライダーのサイズを更新
                col.size = new Vector3(dist, currentWidth, currentWidth);
                // コライダーを線の中心に配置
                col.center = new Vector3(dist / 2f, 0f, 0f);

                // 線を新しい方向と位置に回転・移動させる
                lineObj.transform.rotation = Quaternion.FromToRotation(lineX, lineVec);
                lineObj.transform.position = newStartVec;
            }
        }

        /// <summary>
        /// クリックの対象が最初のピンかどうかを判定するメソッド
        /// </summary>
        public bool IsClickFirstPin(RaycastHit[] hits)
        {
            if (pinList == null) return false;
            return Array.Exists(hits, h => h.collider.gameObject == pinList[0]);
        }

        /// <summary>
        /// エリアが閉じられたかどうかを返すメソッド
        /// </summary>
        public bool IsClosed()
        {
            return isClosed;
        }

        /// <summary>
        /// ピンを削除するメソッド
        /// </summary>
        public void ClearPins()
        {
            foreach (var pin in pinList)
            {
                GameObject.Destroy(pin);
            }
            pinList.Clear();
        }

        /// <summary>
        /// ラインを削除するメソッド
        /// </summary>
        public void ClearLines()
        {
            foreach (var line in lineList)
            {
                GameObject.Destroy(line);
            }
            lineList.Clear();
        }

        /// <summary>
        /// ピンのインデックスを検索するメソッド
        /// </summary>
        public int FindPinIndex(GameObject pin)
        {
            int index = pinList.IndexOf(pin);
            if (index == -1)
            {
                Debug.LogWarning("ピンが見つかりません。");
            }
            return index;
        }

        /// <summary>
        /// ラインのインデックスを検索するメソッド
        /// </summary>
        public int FindLineIndex(GameObject line)
        {
            int index = lineList.IndexOf(line);
            if (index == -1)
            {
                Debug.LogWarning("ラインが見つかりません。");
            }
            return index;
        }

        /// <summary>
        /// PinとLineのTransformをカメラの距離に応じて変更するメソッド
        /// </summary>
        public void ZoomPinLine(float scroll)
        {
            // スクロール量に応じてラインの幅を増減
            if (scroll > 0)
            {
                widthValue += widthStep;
                scaleValue += scaleStep;

            }
            else if (scroll < 0)
            {
                widthValue -= widthStep;
                scaleValue -= scaleStep;
            }

            // 幅が最小値・最大値を超えないように制限
            currentWidth = scaleValue <= maxScale ? widthValue : maxWidth;
            currentWidth = widthValue >= minWidth ? currentWidth : minWidth;
            currentScale = scaleValue <= maxScale ? scaleValue : maxScale;
            currentScale = scaleValue >= minScale ? currentScale : minScale;

            // Lineのスケールを更新
            foreach (var lineObject in lineList)
            {
                LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
                // LineRendererの幅を調整
                lineRenderer.startWidth = currentWidth;
                lineRenderer.endWidth = currentWidth;
            }
            // Pinのスケールを更新
            foreach (var pinObject in pinList)
            {
                pinObject.transform.localScale = new Vector3(
                    currentScale,
                    currentScale,
                    currentScale
                );
            }
        }

        /// <summary>
        /// ラインオブジェクトが他のラインオブジェクトと交差しているかどうかを判定するメソッド
        /// </summary>
        public bool IsIntersected()
        {
            for (int i = 0; i < lineList.Count; i++)
            {
                for (int j = i + 2; j < lineList.Count; j++)
                {
                    // 最後のラインと最初のラインは隣接しているため交差判定を行わない
                    if (i == 0 && j == lineList.Count - 1) continue;

                    if (AreLinesOverlappingAccurate(lineList[i], lineList[j]))
                    {
                        Debug.Log(i + "&" + j);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 動かしたピンによってラインオブジェクトが他のラインオブジェクトと交差したかどうかを判定するメソッド
        /// </summary>
        public bool IsIntersectedByPin(GameObject pin)
        {
            int index = FindPinIndex(pin);
            if (index == -1) return false;

            int lineID1 = index;
            int lineID2 = index == 0 ? lineList.Count - 1 : index - 1 ;

            for (int i = 0; i < lineList.Count; i++)
            {
                // 自身と隣接するラインは交差判定を行わない
                if (i == lineID1 || i == lineID1 + 1 || i == lineID2) continue;
                if(lineID1 == lineList.Count - 1 && i == 0) continue;

                if (AreLinesOverlappingAccurate(lineList[i], lineList[lineID1]))
                {
                    return true;
                }           
            }
            for (int i = 0; i < lineList.Count; i++)
            {
                // 自身と隣接するラインは交差判定を行わない
                if (i == lineID2 || i == lineID1 || i == lineID2 - 1) continue;
                if (lineID2 == 0 && i == lineList.Count - 1) continue;

                if (AreLinesOverlappingAccurate(lineList[i], lineList[lineID2]))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// ラインオブジェクト同士の重なりを判定するメソッド
        /// </summary>
        private bool AreLinesOverlappingAccurate(GameObject line1, GameObject line2)
        {
            BoxCollider collider1 = line1.GetComponent<BoxCollider>();
            collider1.size = new Vector3(collider1.size.x, lineColliderHeight, lineColliderWidth);
            BoxCollider collider2 = line2.GetComponent<BoxCollider>();
            collider2.size = new Vector3(collider2.size.x, lineColliderHeight, lineColliderWidth);


            // コライダーのトランスフォーム情報を使用して交差を判定
            bool isOverlap = Physics.ComputePenetration(
                collider1, collider1.transform.position, collider1.transform.rotation,
                collider2, collider2.transform.position, collider2.transform.rotation,
                out _, out _
            );

            collider1.size = new Vector3(collider1.size.x, currentWidth, currentWidth);
            collider2.size = new Vector3(collider2.size.x, currentWidth, currentWidth);

            return isOverlap;
        }
    }
}
