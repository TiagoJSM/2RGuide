using Assets._2RGuide.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    static class Nav2RGuideSettingsRegister
    {
        public static readonly string SettingsPath = "Project/Nav2RGuide Settings";  

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var fields = Nav2RGuideSettings.SettingFields;

            var provider = new SettingsProvider(SettingsPath, SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    var settings = GetSerializedSettings();

                    foreach (var field in fields)
                    {
                        EditorGUILayout.PropertyField(settings.FindProperty(field));
                    }

                    settings.ApplyModifiedPropertiesWithoutUndo();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(fields.Select(ObjectNames.NicifyVariableName))
            };

            return provider;
        }

        internal static Nav2RGuideSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<Nav2RGuideSettings>(Nav2RGuideSettings.Nav2RGuideSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<Nav2RGuideSettings>();
                CreateDirectories(Nav2RGuideSettings.SettingsPath);
                AssetDatabase.CreateAsset(settings, Nav2RGuideSettings.Nav2RGuideSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        private static void CreateDirectories(string path)
        {
            var entries = path.Split('/');

            if (entries.Length == 0)
            {
                return;
            }

            var parentPath = entries[0];

            for (var idx = 1; idx < entries.Length; idx++)
            {
                var entry = entries[idx];
                var dir = $"{parentPath}/{entry}";

                if (!AssetDatabase.IsValidFolder(dir))
                {
                    AssetDatabase.CreateFolder(parentPath, entry);
                }

                parentPath = dir;
            }
        }
    }
}