using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    public class ChangeLoadMode
    {
        SaveSystem_Assets saveSystem_Assets;
        public void CreateSaveSystemInstance(SaveSystem saveSystem)
        {
            // 各データ(アセット、マテリアルなど)のセーブシステムに関するクラスの初期化
            saveSystem_Assets = new SaveSystem_Assets();
            saveSystem_Assets.InstantiateSaveSystem(saveSystem);
        }

        public void SetLoadMode(string dropdownValue)
        {
            // 各クラスの設定を更新する
            SetLoadMode_Assets(dropdownValue);
        }

        public void SetLoadMode_Assets(string dropdownValue)
        {
            saveSystem_Assets.SetLoadMode(dropdownValue);
        }
    }
}
