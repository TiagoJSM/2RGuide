using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

namespace _2RGuide.Editor
{
    // Create a new type of Settings Asset.
    class Nav2RGuideSettings : ScriptableObject
    {
        static class Nav2RGuideSettingsIMGUIRegister
        {
            [SettingsProvider]
            public static SettingsProvider CreateMyCustomSettingsProvider()
            {
                var fields = new string[]
                {
                    nameof(_maxDropHeight),
                    nameof(_jumpDropHorizontalDistance),
                    nameof(_maxSlope),
                    nameof(_maxJumpHeight),
                    nameof(_segmentDivisionDistance),
                    nameof(_oneWayPlatformMask),
                    nameof(_segmentMaxHeight)
                };

                var provider = new SettingsProvider("Project/Nav2RGuide Settings", SettingsScope.Project)
                {
                    guiHandler = (searchContext) =>
                    {
                        var settings = Nav2RGuideSettings.GetSerializedSettings();

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
        }

        public static string SettingsPath = "Assets/Settings/Editor";
        public static string Nav2RGuideSettingsPath = $"{SettingsPath}/Nav2RGuideSettings.asset";

        [SerializeField]
        private float _maxDropHeight = 10.0f;
        [SerializeField]
        private float _jumpDropHorizontalDistance = 1.0f;
        [Range(0.0f, 90.0f)]
        [SerializeField]
        private float _maxSlope = 60.0f;
        [SerializeField]
        private float _maxJumpHeight;
        [SerializeField]
        private float _segmentDivisionDistance = 1.0f;
        [SerializeField]
        private LayerMask _oneWayPlatformMask;
        [SerializeField]
        private float _segmentMaxHeight = 50.0f;

        public float MaxDropHeight => _maxDropHeight;
        public float JumpDropHorizontalDistance => _jumpDropHorizontalDistance;
        public float MaxSlope => _maxSlope;
        public float MaxJumpHeight => _maxJumpHeight;
        public float SegmentDivisionDistance => _segmentDivisionDistance;
        public LayerMask OneWayPlatformMask => _oneWayPlatformMask;
        public float SegmentMaxHeight => _segmentMaxHeight;

        internal static Nav2RGuideSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<Nav2RGuideSettings>(Nav2RGuideSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<Nav2RGuideSettings>();
                CreateDirectories(SettingsPath);
                AssetDatabase.CreateAsset(settings, Nav2RGuideSettingsPath);
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