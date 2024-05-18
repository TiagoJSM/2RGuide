using Assets._2RGuide.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._2RGuide.Editor
{
    [CustomEditor(typeof(NavWorld))]
    public class WorldEditor : UnityEditor.Editor
    {
        [InitializeOnLoad]
        private static class AutoNavBaker
        {
            static AutoNavBaker()
            {
                EditorSceneManager.sceneSaving += OnSceneSaving;
            }

            private static void OnSceneSaving(Scene scene, string path)
            {
                //NavBaker.BakePathfindingInBackground();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Bake Pathfinding"))
            {
                BakePathfinding();
            }
        }

        private void BakePathfinding()
        {
            NavBaker.BakePathfindingInBackground();
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy, typeof(NavWorld))]
        private static void RenderCustomGizmo(NavWorld objectTransform, GizmoType gizmoType)
        {
            var navWorld = UnityEngine.Object.FindObjectOfType<NavWorld>();

            if (navWorld != null)
            {
                EditorNavDrawer.RenderWorldNav(navWorld);
            }
        }
    }
}