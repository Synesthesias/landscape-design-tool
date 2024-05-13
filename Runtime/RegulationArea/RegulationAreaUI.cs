using Landscape2.Runtime.UiCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class RegulationAreaUI : ISubComponent
    {
        /// <summary> 景観計画エリアを新規作成するボタンです。 </summary>
        public Button AreaCreateButton { get; }
        
        /// <summary> エリアの新規作成モード中に表示すべきUIです。 </summary>
        public VisualElement AreaCreateModeUI { get; }

        /// <summary> エリアの新規作成モードで、輪郭線の選択を完了しエリアを決定するボタンです。 </summary>
        private readonly Button completeContourSelectButton;

        private IRAMode currentMode;
        
        public RegulationAreaUI()
        {
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("RegulationUI");
            AreaCreateButton = uiRoot.Q<Button>("AreaCreateButton");
            AreaCreateModeUI = uiRoot.Q("AreaCreateModeUI");
            completeContourSelectButton = AreaCreateModeUI.Q<Button>("CompleteContourSelectButton");
            AreaCreateButton.clicked += () => SwitchMode(new RACreateMode(this));
            completeContourSelectButton.clicked += CompleteContourSelect;
            SwitchMode(new RANormalMode(this));
        }
        public void Update(float deltaTime)
        {
            
        }

        public void SwitchMode(IRAMode nextMode)
        {
            currentMode = nextMode;
            nextMode.OnModeSwitch();
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        private void CompleteContourSelect()
        {
            if (currentMode is RACreateMode mode)
            {
                mode.CompleteContourSelect();
            }
            else
            {
                Debug.LogError("景観エリアの作成モードでないのに、エリア選択完了が通知されました。");
            }
        }
    }

    /// <summary>
    /// <see cref="RegulationAreaUI"/>の動作モードを規定するインターフェイスです。
    /// RAはRegulationAreaの略です。
    /// </summary>
    public interface IRAMode
    {
        public void OnModeSwitch();
        public void Update();
    }

    public abstract class RAModeBase : IRAMode
    {
        protected RegulationAreaUI ui;

        protected RAModeBase(RegulationAreaUI ui)
        {
            this.ui = ui;
        }

        public abstract void OnModeSwitch();
        public abstract void Update();
    }

    public class RANormalMode : RAModeBase
    {

        public RANormalMode(RegulationAreaUI ui) : base(ui)
        {
        }
        
        public override void OnModeSwitch()
        {
            ui.AreaCreateButton.style.display = DisplayStyle.Flex;
            ui.AreaCreateModeUI.style.display = DisplayStyle.None;
        }

        public override void Update()
        {
            
        }
    }

    public class RACreateMode : RAModeBase
    {
        public RACreateMode(RegulationAreaUI ui) : base(ui){}
        public override void OnModeSwitch()
        {
            ui.AreaCreateButton.style.display = DisplayStyle.None;
            ui.AreaCreateModeUI.style.display = DisplayStyle.Flex;
        }

        public override void Update()
        {
            
        }

        public void CompleteContourSelect()
        {
            ui.SwitchMode(new RANormalMode(ui));
        }
    }
}
