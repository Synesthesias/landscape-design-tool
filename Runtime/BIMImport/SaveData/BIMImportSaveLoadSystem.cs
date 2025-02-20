using System.Collections.Generic;
using System.Linq;
using ToolBox.Serialization;

namespace Landscape2.Runtime
{
    public class BIMImportSaveLoadSystem
    {
        List<BIMImportSaveData> saveDatas = new();

        public List<BIMImportSaveData> SaveDataList => saveDatas;

        public System.Action<List<BIMImportSaveData>> loadCallback = new(_ => { });

        public BIMImportSaveLoadSystem(SaveSystem saveSystem)
        {
            saveSystem.SaveEvent += Save;
            saveSystem.LoadEvent += Load;
        }

        public bool AddSaveData(BIMImportSaveData data)
        {
            // listに同名のassetがあったら、削除して追加する

            RemoveSaveData(data.Name);

            saveDatas.Add(data);

            return true;
        }

        public bool RemoveSaveData(string name)
        {
            var asset = saveDatas.Where(x => x.Name == name).FirstOrDefault();
            if (asset == null)
            {
                return false;
            }

            saveDatas.Remove(asset);
            return true;
        }


        public void Save(string projectID)
        {
            DataSerializer.Save("BIM", saveDatas);
        }

        /// <summary>
        /// Load -> BIMIMportSaveLoadSystem.SaveDataListの読み出しの順で呼び出す
        /// </summary>
        public void Load(string projectID)
        {
            var data = DataSerializer.Load<List<BIMImportSaveData>>("BIM");
            saveDatas = new(data);
            loadCallback?.Invoke(saveDatas);
        }
    }
}
