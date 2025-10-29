using UnityEngine;
using PlateauToolkit.Sandbox;
using PlateauToolkit.Sandbox.Runtime;

namespace Landscape2.Runtime
{
    /// <summary>
    /// アセット情報のJson保存基底クラス
    /// </summary>
    public class ArrangementJsonSaveComponent : MonoBehaviour
    {
        public virtual string GetJsonSaveData()
        {
            return "";
        }

        public virtual void Apply(string jsonData)
        {
        }
    }
}