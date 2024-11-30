using Assets._2RGuide.Runtime;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    public class MenuItems
    {
        [MenuItem("Window/General/Rebake All NavWorlds")]
        static void RebakeAll()
        {
            var currentScenePath = EditorSceneManager.GetActiveScene().path;
            var scenes = EditorBuildSettings.scenes;

            foreach (var scene in scenes)
            {
                Debug.Log($"Building {scene.path}");
                EditorSceneManager.OpenScene(scene.path);
                var world = GameObject.FindObjectOfType<NavWorld>();

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