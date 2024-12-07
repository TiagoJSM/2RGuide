using Assets._2RGuide.Runtime;
using System;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    [CustomEditor(typeof(GuideAgent))]
    public class GuideAgentEditor : UnityEditor.Editor
    {
        private readonly Rect WindowDimension = new Rect(20f, 40f, 180f, 160f);
        private int? _windowId;

        private GUIStyle DebugLabelTitleStyle
        {
            get
            {
                var style = new GUIStyle(EditorStyles.largeLabel);
                style.fontStyle = FontStyle.Bold;
                return style;
            }
        }

        private GUIStyle DebugLabelValueStyle
        {
            get
            {
                var style = new GUIStyle(EditorStyles.largeLabel);
                style.margin.left = 20;
                return style;
            }
        }

        private void OnSceneGUI()
        {
            if(Application.isPlaying)
            {
                _windowId = _windowId ?? GUIUtility.GetControlID(FocusType.Passive);
                GUI.Window(_windowId.Value, WindowDimension, DrawWindowContent, "GuideAgent State");
            }
        }

        private void DrawWindowContent(int id)
        {
            var agent = (GuideAgent)target;
            DrawWindowElement("State", agent.Status.ToString());
            if (agent.CurrentTargetPosition != null)
            {
                DrawWindowElement("Current Target Position", agent.CurrentTargetPosition.Value.ToString("F6"));
            }

            if (agent.CurrentConnectionType != null)
            {
                DrawWindowElement("Current Connection Type", agent.CurrentConnectionType.Value.ToString());
            }
        }

        private void DrawWindowElement(string label, string value)
        {
            GUILayout.Label(label, DebugLabelTitleStyle);
            GUILayout.Label(value, DebugLabelValueStyle);
        }
    }
}