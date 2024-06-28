using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    public class ChangeLoadMode
    {
        AssetsSubscribeSaveSystem assetsSubscribeSaveSystem;
        
        public void CreateSaveSystemInstance(SaveSystem saveSystem)
        {
            // 各データ(アセット、マテリアルなど)のセーブシステムに関するクラスの初期化
            assetsSubscribeSaveSystem = new AssetsSubscribeSaveSystem();
            assetsSubscribeSaveSystem.InstantiateSaveSystem(saveSystem);
        }
    }
}
