using Assets._2RGuide.Runtime;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    [CustomPropertyDrawer(typeof(NavTag))]
    public class NavTagDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var tags = GetTags();
            if (tags.Length == 0)
            {
                return EditorGUIUtility.singleLineHeight * 2;
            }
            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var tags = GetTags();
            var prop = property.FindPropertyRelative(NavTag.TagPath);

            if (tags.Length == 0)
            {
                var textRect = new Rect(position.x, position.y, position.width, position.height / 2);
                var buttonRect = new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2);

                GUI.Label(textRect, "Add nav tags in settings");
                if (GUI.Button(buttonRect, "Open settings"))
                {
                    SettingsService.OpenProjectSettings(Nav2RGuideSettingsRegister.SettingsPath);
                }
            }
            else
            {
                EditorGUI.BeginProperty(position, label, prop);
                prop.intValue = EditorGUI.Popup(position, "Nav tag", prop.intValue, tags);
                EditorGUI.EndProperty();
            }
        }

        private string[] GetTags()
        {
            var instance = Nav2RGuideSettingsRegister.GetOrCreateSettings();
            var navTagSettings = instance.NavTagsSettings;
            return navTagSettings.Select(s => s.Tag).ToArray();
        }
    }
}