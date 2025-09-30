namespace Whitesock
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEditor;

    class CloudBuildSettings : EditorWindow
    {
        #region FIELDS

        private static CloudBuildSettings window;
        private static BuildSettingsEditor settings;
        
        private string projectName;
        private string ProjectName
        {
            get
            {
                projectName = PlayerPrefs.GetString(GetBaseProjectName());
                if (string.IsNullOrEmpty(projectName))
                {
                    projectName = GetBaseProjectName();
                    PlayerPrefs.SetString(GetBaseProjectName(), projectName);
                }
                return projectName;
            }
            set
            {
                if (projectName != value)
                {
                    projectName = value;
                    PlayerPrefs.SetString(GetBaseProjectName(), projectName);
                }
            }
        }

        #endregion FIELDS

        #region UTILITY METHODS
        static string GetBaseProjectName()
        {
            return Directory.GetParent(Application.dataPath).Name;
        }
        #endregion UTILITY METHODS
        
        #region EDITOR VIEW METHODS

        [MenuItem("Whitesock/CloudBuild/Change Settings", false, -239)]
        public static void Init()
        {
            window = GetWindow<CloudBuildSettings>("Cloud Build Settings", true);
            window.maxSize = new Vector2(601, 401);
            window.minSize = new Vector2(600, 400);

            LoadJSONSettings();
            
            GUI.FocusWindow(window.GetInstanceID());
        }
        private void OnGUI()
        {            
            if(settings == null)
                LoadJSONSettings();
            
            GUILayout.Label("Settings:");
            for (int i = 0; i < settings.Platforms.Count; i++)
            {
                settings.PlatformsVisiblity[i] = EditorGUILayout.BeginFoldoutHeaderGroup(settings.PlatformsVisiblity[i], settings.Platforms[i].platformName);
                if (settings.PlatformsVisiblity[i])
                {
                    settings.Platforms[i].platformName = EditorGUILayout.TextField("Platform name:", settings.Platforms[i].platformName);
                    settings.Platforms[i].targetPlatform = EditorGUILayout.TextField("Target platform:", settings.Platforms[i].targetPlatform);
                    settings.Platforms[i].configFile = EditorGUILayout.TextField("Config file:", settings.Platforms[i].configFile);

                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
    
            if (GUILayout.Button("Save Settings"))
                SaveSettings();
        }
        #endregion EDITOR VIEW METHODS
        
        #region LOAD

        static void LoadJSONSettings()
        {
            string dataPath = Application.dataPath;
            string settingsPath = dataPath + "/../../.github/config/platform-matrix.json";
            if (File.Exists(settingsPath))
            {
                settings ??= new BuildSettingsEditor();
                settings.Clear();
                settings.AddRange(JsonUtility.FromJson<BuildSetting>(File.ReadAllText(settingsPath)).include, false);
            }
            else
            {
                settings =  new BuildSettingsEditor();
            }
        }

        #endregion
        
        #region  SAVE
        void SaveSettings()
        {
            window.Close();
        }
        #endregion SAVE
    }

    [Serializable]
    class BuildSetting
    {
        public PlatformSettings[] include;
    }

    [Serializable]
    class BuildSettingsEditor
    {
        public List<PlatformSettings> Platforms;
        public List<bool> PlatformsVisiblity;

        public BuildSettingsEditor()
        {
            Platforms = new List<PlatformSettings>();
            PlatformsVisiblity = new List<bool>();
        }

        public void Add(PlatformSettings platform, bool visibility = true)
        {
            Platforms.Add(platform);
            PlatformsVisiblity.Add(visibility);
        }

        public void AddRange(PlatformSettings[] platforms, bool visibility = true)
        {
            Platforms.AddRange(platforms);
            for(int i = 0; i < platforms.Length; i++)
                PlatformsVisiblity.Add(visibility);
        }

        public void Clear()
        {
            Platforms?.Clear();
            PlatformsVisiblity?.Clear();
        }
    }
    [Serializable]
    class PlatformSettings
    {
        public string platformName;
        public string targetPlatform;
        public string configFile;
    }

}