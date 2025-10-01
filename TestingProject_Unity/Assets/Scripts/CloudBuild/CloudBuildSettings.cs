namespace Whitesock
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEditor.VersionControl;

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

        private List<string> buildTargets = new List<string>();
        private Vector2 scrollPos;

        List<string> BuildTargets
        {
            get
            {
                buildTargets ??= new List<string>();
                if(buildTargets.Count == 0)
                    buildTargets.AddRange(Enum.GetNames(typeof(BuildTarget)));
                return buildTargets;
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
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Settings:", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                settings.Add(new PlatformSettings(), true);
            EditorGUILayout.EndHorizontal();
            HorizontalLine(Color.grey);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            for (int i = 0; i < settings.Platforms.Count; i++)
            {
                var leftStyle = EditorStyles.foldout;
                leftStyle.alignment = TextAnchor.MiddleLeft;
                leftStyle.stretchWidth = false;
                EditorGUILayout.BeginHorizontal();
                settings.PlatformsVisiblity[i] = EditorGUILayout.BeginFoldoutHeaderGroup(settings.PlatformsVisiblity[i], settings.Platforms[i].platformName, leftStyle);
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                    settings.Remove(settings.Platforms[i]);
                EditorGUILayout.EndHorizontal();
                if (settings.PlatformsVisiblity[i])
                {
                    settings.Platforms[i].platformName = EditorGUILayout.TextField("Platform name:", settings.Platforms[i].platformName);
                    int index = BuildTargets.IndexOf(settings.Platforms[i].targetPlatform);
                    index = index < 0 ? 0 : index;
                    settings.Platforms[i].targetPlatform = BuildTargets[EditorGUILayout.Popup("Target platform:", index, BuildTargets.ToArray(),  EditorStyles.popup)]; 
                    //EditorGUILayout.TextField("Target platform:", settings.Platforms[i].targetPlatform);
                    settings.ConfigFiles[i] = EditorGUILayout.ObjectField("Build Profile:", settings.ConfigFiles[i], typeof(BuildProfile), false) as BuildProfile;
                    settings.Platforms[i].configFile = AssetDatabase.GetAssetPath(settings.ConfigFiles[i]);
                    EditorGUILayout.LabelField("Build Profile Asset Path:", settings.Platforms[i].configFile);

                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Save Settings"))
                SaveSettings();
            
        }
        #endregion EDITOR VIEW METHODS
        
        #region LOAD

        static void LoadJSONSettings()
        {
            //TODO: convert PlatformSettings.configFile asset path in build profile file
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
        #region UTILITY
        static void HorizontalLine (Color color) {
            GUIStyle horizontalLine;
            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset( 0, 0, 4, 4 );
            horizontalLine.fixedHeight = 1;
            horizontalLine.fixedWidth = 150;
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box( GUIContent.none, horizontalLine);
            GUI.color = c;
        }
        #endregion
    }

    #region DATAS
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
        public List<BuildProfile> ConfigFiles;

        public BuildSettingsEditor()
        {
            Platforms = new List<PlatformSettings>();
            PlatformsVisiblity = new List<bool>();
            ConfigFiles =  new List<BuildProfile>();
        }

        public void Add(PlatformSettings platform, bool visibility = true)
        {
            Platforms.Add(platform);
            PlatformsVisiblity.Add(visibility);
            ConfigFiles.Add(ScriptableObject.CreateInstance<BuildProfile>());
        }

        public void Remove(PlatformSettings platform)
        {
            int index =  Platforms.IndexOf(platform);
            Platforms.Remove(platform);
            PlatformsVisiblity.RemoveAt(index);
            ConfigFiles.RemoveAt(index);
        }

        public void AddRange(PlatformSettings[] platforms, bool visibility = true)
        {
            Platforms.AddRange(platforms);
            for (int i = 0; i < platforms.Length; i++)
            {   
                
                var currentPlatform = platforms[i];
                BuildProfile profile = ScriptableObject.CreateInstance<BuildProfile>();
                var loadedFile = (BuildProfile)AssetDatabase.LoadAssetAtPath(currentPlatform.configFile, typeof(BuildProfile));
                if(loadedFile != null)
                    profile = loadedFile;
                PlatformsVisiblity.Add(visibility);
                ConfigFiles.Add(profile);
            }
        }
        public void Clear()
        {
            Platforms?.Clear();
            PlatformsVisiblity?.Clear();
            ConfigFiles?.Clear();
        }
    }
    [Serializable]
    class PlatformSettings
    {
        public string platformName;
        public string targetPlatform;
        public string configFile;
    }
    #endregion DATAS
}