using System.Collections.Generic;
using Landscape2.Runtime.CameraPositionMemory;
using Landscape2.Runtime.UiCommon;
using Landscape2.Runtime.WeatherTimeEditor;
using Landscape2.Runtime.LandscapePlanLoader;
using Landscape2.Runtime.BuildingEditor;
using UnityEngine.UIElements;
using UnityEngine;
using Cinemachine;
using Landscape2.Runtime.GisDataLoader;
using Landscape2.Runtime.WalkerMode;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.Processors;

namespace Landscape2.Runtime.LicenseAuth
{
    public class LicenseAuthSubComponents : MonoBehaviour
    {
        private List<ISubComponent> subComponents = new();

        private void Awake()
        {
            // 必要な機能をここに追加します
            subComponents = new List<ISubComponent>
            {
                new LicenseAuthPageRouter(),
            };
        }

        private void Start()
        {
            foreach (var c in subComponents)
            {
                c.Start();
            }
        }

        private void OnEnable()
        {
            foreach (var c in subComponents)
            {
                c.OnEnable();
            }
        }

        private void Update()
        {
            foreach (var c in subComponents)
            {
                c.Update(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            foreach (var c in subComponents)
            {
                c.LateUpdate(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            foreach (var c in subComponents)
            {
                c.OnDisable();
            }
        }
    }
}