using Assets._2RGuide.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    public class MenuItems : ScriptableObject
    {
        [MenuItem("Window/General/Rebake All NavWorlds")]
        static void RebakeAll()
        {
            var currentScenePath = EditorSceneManager.GetActiveScene().path;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                Debug.Log($"Building {scene.path}");
                EditorSceneManager.OpenScene(scene.path);
                var world = FindObjectOfType<NavWorld>();

                if (world != null)
                {
                    NavBaker.BakePathfinding(world);
                    EditorUtility.SetDirty(world);
                    EditorSceneManager.SaveOpenScenes();
                }
            }

            EditorSceneManager.OpenScene(currentScenePath);
        }
    }
}