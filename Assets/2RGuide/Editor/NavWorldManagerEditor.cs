using Assets._2RGuide.Runtime;
using UnityEditor;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    [CustomEditor(typeof(NavWorldManager))]
    public class NavWorldManagerEditor : UnityEditor.Editor
    {
        private readonly Rect WindowDimension = new Rect(20f, 40f, 180f, 160f);
        private int? _windowId;
        private Vector2 _scrollPosition = Vector2.zero;

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
            if (Application.isPlaying)
            {
                _windowId = _windowId ?? GUIUtility.GetControlID(FocusType.Passive);
                GUI.Window(_windowId.Value, WindowDimension, DrawWindowContent, "NavWorldManager State");
            }
        }

        private void DrawWindowContent(int id)
        {
            var navWorldManager = (NavWorldManager)target;
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            foreach(var caller in navWorldManager.Callers)
            {
                GUILayout.Label(caller.name, DebugLabelValueStyle);
            }
            GUILayout.EndScrollView();
        }
    }
}
