using Assets.Scripts;
using Assets.Scripts._2RGuide;
using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using Clipper2Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

namespace Assets.Editor
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

        private void OnSceneDirtied(Scene _)
        {
            Debug.Log("thing");
        }
    }
}