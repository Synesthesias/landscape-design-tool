using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace Landscape2.Runtime.SaveData
{
    public abstract class ProjectSaveData
    {
        // データ
        protected Dictionary<string, List<string>> Datas = new();
        
        public void Add(string projectID, string id)
        {
            if (Datas.TryGetValue(projectID, out var datas))
            {
                if (!datas.Contains(id))
                {
                    datas.Add(id);
                    Datas[projectID] = datas;
                }
            }
            else
            {
                Datas.Add(projectID, new List<string> { id });
            }
        }

        public void Delete(string id)
        {
            foreach (var key in Datas.Keys.ToList())
            {
                if (Datas[key].Contains(id))
                {
                    Datas[key].Remove(id);
                    if (Datas[key].Count == 0)
                    {
                        Datas.Remove(key);
                    }
                    break;
                }
            }
        }
        
        public string GetProjectID(string id)
        {
            foreach (var keyValuePair in Datas)
            {
                if (keyValuePair.Value.Contains(id))
                {
                    return keyValuePair.Key;
                }
            }
            return "";
        }

        public bool TryCheckData(string projectID, string id)
        {
            if (Datas.TryGetValue(projectID, out var data))
            {
                return data.Contains(id);
            }
            return false;
        }
    }
}