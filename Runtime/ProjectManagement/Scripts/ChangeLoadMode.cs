using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Landscape2.Runtime.LandscapePlanLoader;

namespace Landscape2.Runtime
{
    public class ChangeLoadMode
    {
        AssetsSubscribeSaveSystem assetsSubscribeSaveSystem;
        // LandscapePlanSaveSystem landscapePlanSaveSystem;

        public void CreateSaveSystemInstance(SaveSystem saveSystem)
        {
            // 各データ(アセット、マテリアルなど)のセーブシステムに関するクラスの初期化
            assetsSubscribeSaveSystem = new AssetsSubscribeSaveSystem();
            assetsSubscribeSaveSystem.InstantiateSaveSystem(saveSystem);

            // landscapePlanSaveSystem = new LandscapePlanSaveSystem();
            // landscapePlanSaveSystem.InstantiateSaveSystem(saveSystem);

        }
    }
}
