using UnityEngine.UIElements;
using Landscape2.Runtime.UiCommon;

namespace Landscape2.Runtime.LandscapePlanLoader
{
    /// <summary>
    /// LandscapePlanLoaderUI.uxmlのプレゼンタクラス
    /// </summary>
    public sealed class LandscapePlanEditorUI : ISubComponent
    {
        // UI Elements
        IntegerField targetAreaInput;
        Button startEditButton;
        Slider heightSlider;
        FloatField heightInput;
        Button resetButton;
        Button endEditButton;

        AreaEditManager areaEditManager;


        public LandscapePlanEditorUI()
        {
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("LandscapePlanEditorUI");
            areaEditManager = new AreaEditManager();

            targetAreaInput = uiRoot.Q<IntegerField>("TargetAreaInput");
            startEditButton = uiRoot.Q<Button>("StartEditButton");
            heightSlider = uiRoot.Q<Slider>("HeightSlider");
            heightInput = uiRoot.Q<FloatField>("HeightInput");
            resetButton = uiRoot.Q<Button>("ResetButton");
            endEditButton = uiRoot.Q<Button>("EndEditButton");

            startEditButton.clicked += OnClickStartEdit;
            endEditButton.clicked += OnClickEndEdit;
            resetButton.clicked += OnClickReset;
            heightSlider.RegisterValueChangedCallback(evt => SliderChangeHeight());
            heightInput.RegisterValueChangedCallback(evt => InputFieldChangeHeight());
        }
        public void OnClickStartEdit()
        {
            int index = targetAreaInput.value;
            areaEditManager.StartEdit(index);

            heightSlider.highValue = areaEditManager.GetMaxHeight();
            heightSlider.value = areaEditManager.GetLimitHeight();
            heightInput.value = heightSlider.value;
        }

        public void OnClickEndEdit()
        {
            areaEditManager.StopEdit();
        }

        public void SliderChangeHeight()
        {
            areaEditManager.ChangeHeight(heightSlider.value);
            heightInput.value = heightSlider.value;
        }

        public void InputFieldChangeHeight()
        {
            areaEditManager.ChangeHeight(heightInput.value);
            heightSlider.value = heightInput.value;
        }

        public void OnClickReset()
        {
            int index = targetAreaInput.value;
            areaEditManager.ResetProperty(index);
            heightSlider.value = areaEditManager.GetLimitHeight();
            heightInput.value = heightSlider.value;
        }

        public void Start()
        {
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

