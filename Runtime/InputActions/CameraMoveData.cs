using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    [CreateAssetMenu(fileName ="CameraMoveSpeedData", menuName ="Landscape-Design-Tool-2/CameraMoveSpeedData")]
    public class CameraMoveData: ScriptableObject
    {
        public float horizontalMoveSpeed;
        public float verticalMoveSpeed;
        public float parallelMoveSpeed;
        public float zoomMoveSpeed;
        public float rotateSpeed;
        public float zoomLimit;
        public float heightLimitY;
        public float walkerMoveSpeed;
        public float walkerOffsetYSpeed;
    }
}
