using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class BuildingTRSEditor : ISubComponent
    {
        private EditMode editMode = new();

        BuildingTRSEditorUI trsUI;
        BuildingDeleteListUI deleteListUI;

        GameObject target;

        List<Renderer> disableList = new();



        public BuildingTRSEditor(EditBuilding editBuilding, VisualElement element)
        {
            editMode.OnCancel();
            trsUI = new(editBuilding, element);
            deleteListUI = new(element, this);

            editBuilding.OnBuildingSelected += OnSelectBuilding;

            deleteListUI.OnClickShowButton += (go) =>
            {
                var r = disableList.Where(r => r.gameObject == go).FirstOrDefault();
                if (!r)
                {
                    return;
                }
                r.enabled = true;
                disableList.Remove(r);
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

                r.enabled = false;
                if (!disableList.Contains(r))
                {
                    disableList.Add(r);

                    deleteListUI?.AppendList(r.gameObject);
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
    }
}
