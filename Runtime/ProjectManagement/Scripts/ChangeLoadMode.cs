using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    public class ChangeLoadMode
    {
        SubscribeSaveSystem subscribeSaveSystem;
        public void CreateSaveSystemInstance(SaveSystem saveSystem)
        {
            // 各データ(アセット、マテリアルなど)のセーブシステムに関するクラスの初期化
            subscribeSaveSystem = new SubscribeSaveSystem();
            subscribeSaveSystem.InstantiateSaveSystem(saveSystem);
        }

        public void SetLoadMode(LoadTypeCategory dropdownValue)
        {
            // 各クラスの設定を更新する
            SetLoadMode_Assets(dropdownValue);
        }

        public void SetLoadMode_Assets(LoadTypeCategory loadType)
        {
            subscribeSaveSystem.SetLoadMode(loadType.ToString());
        }
    }
}
