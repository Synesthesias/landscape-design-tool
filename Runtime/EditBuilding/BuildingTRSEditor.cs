using Landscape2.Runtime.BuildingEditor;
using Landscape2.Runtime.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class BuildingTRSEditor : ISubComponent
    {
        const float focusDistanceMultiplyer = 16f;
        private EditMode editMode = new();

        BuildingTRSEditorUI trsUI;
        BuildingDeleteListUI deleteListUI;

        GameObject target;

        List<Renderer> disableList = new();

        GameObjectFocus assetFocus;

        GameObject targetViewObject;





        public BuildingTRSEditor(EditBuilding editBuilding, VisualElement element, LandscapeCamera landscapeCamera)
        {
            editMode.OnCancel();
            trsUI = new(editBuilding, element);
            deleteListUI = new(element, this);

            assetFocus = new(landscapeCamera);
            assetFocus.focusFinishCallback += _ => assetFocus.FocusFinish();

            targetViewObject = new();
            targetViewObject.name = "TargetViewObject";

            editBuilding.OnBuildingSelected += OnSelectBuilding;

            deleteListUI.OnClickShowButton += (go) =>
            {
                Debug.Log($"go => {go.name} : {go.transform.position} : {go.transform.localPosition}", go);
                editBuilding.SetTargetObject(go);
                var bounds = CalculateBounds(go);
                targetViewObject.transform.position = bounds.center;
                assetFocus.Focus(targetViewObject.transform, focusDistanceMultiplyer);

                var r = disableList.Where(r => r.gameObject == go).FirstOrDefault();
                if (!r)
                {
                    return;
                }
                var editComponent = BuildingTRSEditingComponent.TryGetOrCreate(r.gameObject);
                editComponent.ShowBuilding(true);

                disableList.Remove(r);
                
                var gml = CityObjectUtil.GetGmlID(r.gameObject);
                BuildingsDataComponent.SetBuildingDelete(gml, false);
            };

            deleteListUI.OnClickListElement += (go) =>
            {
                editBuilding.SetTargetObject(go);
                var bounds = CalculateBounds(go);
                targetViewObject.transform.position = bounds.center;
                assetFocus.Focus(targetViewObject.transform, focusDistanceMultiplyer);
            };

            trsUI.OnClickDeleteButton += () =>
            {
                if (target == null)
                {
                    Debug.LogWarning($"targetがないです");
                    return;
                }

                if (!target.TryGetComponent<Renderer>(out var r))
                {
                    Debug.LogWarning($"{target.name} : rendererがnullです");
                }

                var editComponent = BuildingTRSEditingComponent.TryGetOrCreate(r.gameObject);
                editComponent.ShowBuilding(false);

                if (!disableList.Contains(r))
                {
                    disableList.Add(r);
                    deleteListUI?.AppendList(r.gameObject, true);

                    var gml = CityObjectUtil.GetGmlID(r.gameObject);
                    BuildingsDataComponent.SetBuildingDelete(gml, true);
                }
            };
            trsUI.OnClickTransButton += () =>
            {
                ChangeEditMode(target, TransformType.Position);
            };
            trsUI.OnClickRotateButton += () =>
            {
                ChangeEditMode(target, TransformType.Rotation);
            };
            trsUI.OnClickScaleButton += () =>
            {
                ChangeEditMode(target, TransformType.Scale);
            };


        }

        void ChangeEditMode(GameObject target, TransformType type)
        {
            editMode.OnCancel();
            //  後で使用するかも知れないので一旦コメントアウトのみ
            // editMode.CreateRuntimeHandle(target, type);
        }

        public void OnSelectBuilding(GameObject select)
        {
            if (select != target)
            {
                if (select != null)
                {
                    ChangeEditMode(select, TransformType.Scale);
                }

            }
            target = select;
        }

        public void OnDisable()
        {
            trsUI?.OnDisable();
            deleteListUI?.OnDisable();
            target = null;
        }

        public void OnEnable()
        {
            trsUI?.OnEnable();
            deleteListUI?.OnEnable();
        }

        public void Update(float deltaTime)
        {

            trsUI?.Update(deltaTime);

            deleteListUI?.ShowListEmpty(disableList.Count < 1);
            deleteListUI?.Update(deltaTime);


            editMode.Update();
        }


        public void Start()
        {
            trsUI?.Start();
            deleteListUI?.Start();
        }

        public void LateUpdate(float deltaTime)
        {
        }


        /// <summary>
        /// 手抜きですがCameraMoveByUserInputsから取ってきた
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Bounds CalculateBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogError("指定されたオブジェクトにRendererが存在しません。");
                return new Bounds(obj.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

    }
}
