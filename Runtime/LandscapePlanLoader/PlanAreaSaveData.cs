using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// 景観区画のセーブデータ項目
    /// </summary>
    [Serializable]
    public struct PlanAreaSaveData
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private float _limitHeight;
        [SerializeField] private float _lineOffset;
        [SerializeField] private Color _color;
        [SerializeField] private float _wallMaxHeight;
        [SerializeField] private List<Vector3> _pointData;

        public int id => _id;
        public string name => _name;
        public float limitHeight => _limitHeight;
        public float lineOffset => _lineOffset;
        public Color color => _color;
        public float wallMaxHeight => _wallMaxHeight;
        public List<Vector3> pointData => _pointData;

        public PlanAreaSaveData(int id, string name, float limitHeight, float lineOffset, Color color, float wallMaxHeight, List<Vector3> pointData)
        {
            _id = id;
            _name = name;
            _limitHeight = limitHeight;
            _lineOffset = lineOffset;
            _color = color;
            _wallMaxHeight = wallMaxHeight;
            _pointData = pointData;
        }
    }
}
