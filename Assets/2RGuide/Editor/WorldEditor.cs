using _2RGuide;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _2RGuide.Editor
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
                var navWorld = UnityEngine.Object.FindObjectOfType<NavWorld>();

                if (navWorld != null)
                {
                    NavBaker.BakePathfinding(navWorld);
                    EditorUtility.SetDirty(navWorld);
                }
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
            var world = (NavWorld)target;
            NavBaker.BakePathfinding(world);
            EditorUtility.SetDirty(world);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var world = (NavWorld)target;
            EditorNavDrawer.RenderWorldNav(world);
        }
    }
}