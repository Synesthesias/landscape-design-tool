using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using LandscapeDesignTool2.Runtime.LandscapePlanLoader;
using Landscape2.Runtime.UiCommon;

namespace Landscape2.Runtime
{
    public class LandscapePlanLoaderUI : ISubComponent
    {
        Button loadButton;
        Button browseButton;
        Label selectedPathLabel;

        LandscapePlanLoadManager landscapePlanLoadManager;

        string selectedPath;

        public LandscapePlanLoaderUI()
        {
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("LandscapePlanLoaderUI");
            landscapePlanLoadManager = new LandscapePlanLoadManager();

            loadButton = uiRoot.Q<Button>("LoadButton");
            browseButton = uiRoot.Q<Button>("FolderBrowseButton");
            selectedPathLabel = uiRoot.Q<Label>("SelectedPathLabel");

            loadButton.clicked +=  OnClickLoadButton;
            browseButton.clicked +=  OnClickBrowseButton;
        }
        public void OnClickLoadButton()
        {
            landscapePlanLoadManager.LoadShapefile(selectedPath);
        }

        public void OnClickBrowseButton()
        {
            string path = landscapePlanLoadManager.BrowseFolder();
            if (path != null)
            {
                selectedPathLabel.text = path;
                selectedPath = path;
            }
        }

        public void Update(float deltaTime)
        {

        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }


    }
}

