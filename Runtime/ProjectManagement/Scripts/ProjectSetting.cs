using System;
using System.Collections.Generic;
using System.Linq;

namespace Landscape2.Runtime
{
    public class ProjectData
    {
        public string projectName;
        public string projectID;
        
        public bool IsDefault => projectName == ProjectSetting.DefaultProjectName;
    }
    
    public class ProjectSetting
    {
        public static string DefaultProjectName = "プロジェクト";
        
        private ProjectData currentProject;
        public ProjectData CurrentProject => currentProject;
        
        private List<ProjectData> projectList = new();
        public List<ProjectData> ProjectList => projectList;

        // 編集モード / 閲覧モード
        private bool isEditMode = true;
        public bool IsEditMode => isEditMode;
        
        public ProjectSetting()
        {
            currentProject = CreateDefaultProject();
        }
        
        public ProjectData CreateDefaultProject()
        {
            return Add(DefaultProjectName);
        }
        
        public ProjectData Add(string projectName)
        {
            var udid = System.Guid.NewGuid().ToString();
            var projectData = new ProjectData
            {
                projectName = projectName,
                projectID = udid
            };
            projectList.Add(projectData);
            return projectData;
        }
        
        public void Remove(string projectID)
        {
            projectList.RemoveAll(x => x.projectID == projectID);
        }
        
        public void SetCurrentProject(string projectID)
        {
            currentProject = projectList.First(x => x.projectID == projectID);
        }
        
        public bool IsCurrentProject(string projectID)
        {
            return currentProject.projectID == projectID;
        }
        
        public ProjectData GetProject(string projectID)
        {
            return projectList.First(x => x.projectID == projectID);
        }

        public ProjectData GetDefaultProject()
        {
            return projectList.First(x => x.IsDefault);
        }

        public void Rename(string projectID, string newName)
        {
            if (projectList.All(x => x.projectID != projectID))
            {
                return;
            }
            
            var project = projectList.First(x => x.projectID == projectID);
            project.projectName = newName;
        }

        public void SetEditMode(bool isEdit)
        {
            isEditMode = isEdit;
        }
    }
}