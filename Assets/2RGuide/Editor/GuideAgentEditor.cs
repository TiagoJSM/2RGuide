using _2RGuide;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    [CustomEditor(typeof(GuideAgent))]
    public class GuideAgentEditor : UnityEditor.Editor
    {
        private GUIStyle DebugLabelTitleStyle
        {
            get
            {
                var style = new GUIStyle(EditorStyles.largeLabel);
                style.fontStyle = FontStyle.Bold;
                return style;
            }
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var agent = (GuideAgent)target;
            
            GUILayout.Label("");
            GUILayout.Label("Debug Info:", DebugLabelTitleStyle);
            GUILayout.Label($"State: {agent.Status}");
            GUILayout.Label($"Current Target Position: {agent.CurrentTargetPosition}");
        }
    }
}