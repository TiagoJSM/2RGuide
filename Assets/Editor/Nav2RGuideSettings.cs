using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Editor
{
    class Nav2RGuideSettings : ScriptableObject
    {
        public const string Nav2RGuideSettingsPath = "Assets/Editor/Nav2RGuideSettings.asset";

        [SerializeField]
        private float _maxDropHeight = 10.0f;
        [SerializeField]
        private float _horizontalDistance = 1.0f;
        [Range(0.0f, 90.0f)]
        [SerializeField]
        private float _maxSlope = 60.0f;

        internal static Nav2RGuideSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<Nav2RGuideSettings>(Nav2RGuideSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<Nav2RGuideSettings>();
                //settings.m_Number = 42;
                //settings.m_SomeString = "The answer to the universe";
                AssetDatabase.CreateAsset(settings, Nav2RGuideSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    static class Nav2RGuideSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateNav2RGuideSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/Nav2RGuideSettings", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Nav 2RGuide Settings",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = Nav2RGuideSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(settings.FindProperty("_maxDropHeight"), new GUIContent("Max Drop Height"));
                    EditorGUILayout.PropertyField(settings.FindProperty("_horizontalDistance"), new GUIContent("Horizontal Distance"));
                    EditorGUILayout.PropertyField(settings.FindProperty("_maxSlope"), new GUIContent("Max Slope"));
                    settings.ApplyModifiedPropertiesWithoutUndo();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Drop", "Height", "Horizontal", "Distance", "Slope" })
            };

            return provider;
        }
    }

    class Nav2RGuideSettingsProvider : SettingsProvider
    {
        private SerializedObject m_CustomSettings;

        class Styles
        {
            public static GUIContent maxDropHeight = new GUIContent("Max Drop Height");
            public static GUIContent horizontalDistance = new GUIContent("Horizontal Distance");
            public static GUIContent maxSlope = new GUIContent("Max Slope");
        }

        const string Nav2RGuideSettingsPath = "Assets/Editor/Nav2RGuideSettings.asset";
        public Nav2RGuideSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(Nav2RGuideSettingsPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            m_CustomSettings = Nav2RGuideSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            // Use IMGUI to display UI:
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("_maxDropHeight"), Styles.maxDropHeight);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("_horizontalDistance"), Styles.horizontalDistance);
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("_maxSlope"), Styles.maxSlope);
            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateNav2RGuideSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new Nav2RGuideSettingsProvider("Project/Nav2RGuideSettingsProvider", SettingsScope.Project);

                // Automatically extract all keywords from the Styles.
                provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}