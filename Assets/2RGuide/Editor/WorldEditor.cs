using Assets._2RGuide.Runtime;
using NUnit.Framework;
using System;
using System.Linq;
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

            GUI.enabled = NavBaker.ReadyToBakeInBackground;
            if (GUILayout.Button("Bake Pathfinding"))
            {
                BakePathfinding();
            }
            if (GUILayout.Button("Save debug scene"))
            {
                SaveDebugScene();
            }
        }

        private void BakePathfinding()
        {
            NavBaker.BakePathfindingInBackground();
        }

        private void SaveDebugScene()
        {
            var scenePath = "Assets/Nav Debug Scene.unity";
            var scene = EditorSceneManager.GetSceneByName("Nav Debug Scene");
            if (scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, true);
            }
            AssetDatabase.DeleteAsset(scenePath);
            var navWorld = FindObjectOfType<NavWorld>();
            var debugScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var newNavWorld = Instantiate(navWorld);
            RemoveAllNonNavDebugComponents(newNavWorld.gameObject);
            EditorSceneManager.SaveScene(debugScene, scenePath);
            EditorSceneManager.MoveGameObjectToScene(newNavWorld.gameObject, debugScene);

            //EditorSceneManager.UnloadSceneAsync(debugScene);
        }

        private readonly Type[] ValidNavDebugComponents = new Type[]
        {
            typeof(Transform),
            typeof(Collider2D),
            typeof(NavWorld),
            typeof(SpriteRenderer),
            typeof(NavTagBounds),
        };

        private void RemoveAllNonNavDebugComponents(GameObject go)
        {
            var components = go.GetComponents<Component>();
            foreach (var item in components)
            {
                if (!ValidNavDebugComponents.Any(c => c.IsAssignableFrom(item.GetType())))
                {
                    Debug.Log($"destroy {item.GetType()}");
                    DestroyImmediate(item);
                }
            }

            for (var i = 0; i < go.transform.childCount; i++)
            {
                RemoveAllNonNavDebugComponents(go.transform.GetChild(i).gameObject);
            }
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