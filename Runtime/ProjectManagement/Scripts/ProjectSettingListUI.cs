using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Runtime
{
    public class ProjectSettingListUI
    {
        private VisualTreeAsset projectItemTreeAsset;
        private ScrollView scrollView;
        
        public bool IsShow => scrollView.style.display == DisplayStyle.Flex;
        
        public UnityEvent<string> OnSelect = new ();
        public UnityEvent<string> OnDelete = new ();
        public UnityEvent<ProjectData> OnSave = new ();
        public UnityEvent<string> OnRename = new ();
        
        public ProjectSettingListUI(VisualElement element)
        {
            scrollView = element.Q<ScrollView>("Project_List");
            var projectBlank = element.Q<VisualElement>("List_Project_blank");
            projectItemTreeAsset = Resources.Load<VisualTreeAsset>("List_Project");
        }
        
        public void Show(bool isShow)
        {
            scrollView.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public void Add(ProjectData projectData)
        {
            var clone = projectItemTreeAsset.CloneTree();
            var label = clone.Q<Label>("ProjectName");
            label.text = projectData.projectName;
            label.name = projectData.projectID;
            
            // プロジェクトを選択
            clone.Q<Button>("Btn_Project").clicked += () =>
            {
                OnSelect.Invoke(projectData.projectID);
                Show(false);
            };
            
            // 保存
            clone.Q<Button>("SaveButton").clicked += () =>
            {
                // ファイルに保存
                OnSave.Invoke(projectData);
            };
            
            // 削除
            clone.Q<Button>("DeleteButton").clicked += () =>
            {
                OnDelete.Invoke(projectData.projectID);
            };
            
            // リネーム
            clone.Q<Button>("RenameButton").clicked += () =>
            {
                // ポップアップを表示
                OnRename.Invoke(projectData.projectID);
            };
            
            scrollView.Add(clone);
        }
        
        public void Rename(string projectID, string newProjectName)
        {
            var label = scrollView.Q<Label>(projectID);
            if (label.text.Contains(("*")))
            {
                newProjectName = $"* {newProjectName}";
            }
            label.text = newProjectName;
        }
        
        public void Delete(string projectID)
        {
            var label = scrollView.Q<Label>(projectID);
            scrollView.Remove(label.parent.parent);
        }
        
        public void Edit(string projectID)
        {
            var label = scrollView.Q<Label>(projectID);
            if (label == null)
            {
                return;
            }
            if (label.text.Contains(("*")))
            {
                return;
            }
            label.text = $"* {label.text}";
        }

        public void Save(string projectID)
        {
            var label = scrollView.Q<Label>(projectID);
            if (label.text.Contains(("*")))
            {
                label.text = label.text.Replace("* ", "");
            }
        }
    }
}