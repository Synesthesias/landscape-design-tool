using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using Landscape2.Runtime.UiCommon;

namespace Landscape2.Runtime.LicenseAuth
{
    public abstract class BaseAuthPage
    {
        public abstract string uxmlName
        {
            get;
        }

        protected VisualElement uiRoot;

        public Action<BaseAuthPage> pageChange;

        public BaseAuthPage()
        {
            uiRoot = new UIDocumentFactory().CreateWithUxmlName(uxmlName);
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }
    }
}