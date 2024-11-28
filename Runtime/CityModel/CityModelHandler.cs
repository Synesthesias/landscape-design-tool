using PLATEAU.CityInfo;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    public class CityModelHandler
    {
        private static List<PLATEAUCityObjectGroup> cityModelList = new ();
        public static List<PLATEAUCityObjectGroup> CityModelList
        {
            get
            {
                if (cityModelList.Count == 0)
                {
                    var cityModels = GameObject.FindObjectsByType<PLATEAUInstancedCityModel>(FindObjectsSortMode.None);
                    foreach (var cityModel in cityModels)
                    {
                        var cityModelObjs = cityModel.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                        foreach (var cityModelObj in cityModelObjs)
                        {
                            cityModelList.Add(cityModelObj);
                        }
                    }
                }
                return cityModelList;
            }
        }

        private const float smoothnessValue = 0.12f;

        public CityModelHandler()
        {
            foreach (var cityModelObj in CityModelList)
            {          
                InitBuilding(cityModelObj);
            }
        }

        private void InitBuilding(PLATEAUCityObjectGroup building)
        {

            if (building.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                foreach (var meshRendererSharedMaterial in meshRenderer.materials)
                {
                    // 各パラメータ調整
                    SetSmoothness(meshRendererSharedMaterial);
                }
            }
        }

        private void SetSmoothness(Material material)
        {
            material.SetFloat("_Smoothness", smoothnessValue);
        }
    }
}