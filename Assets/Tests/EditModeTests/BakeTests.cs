using System.Collections;
using System.Collections.Generic;
using Assets._2RGuide.Editor;
using Assets._2RGuide.Runtime;
using Assets.Tests.EditModeTests;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.Tests.EditModeTests
{
    public class BakeTests
    {
        static readonly string[] Scenes = new[]
        {
            "Assets/Tests/PlayModeTests/MoveToPositionTestScene.unity",
            "Assets/Tests/PlayModeTests/MoveToPositionWithJumpsTestScene.unity",
            "Assets/Tests/PlayModeTests/MoveToPositionOneWayJumpTestScene.unity",
            "Assets/Tests/PlayModeTests/MoveToAdjacentSegmentTestScene.unity",
            "Assets/Tests/PlayModeTests/MoveToAdjacentSegmentButCloserNodeTestScene.unity",
            "Assets/Tests/PlayModeTests/MoveToPositionThroughObstacleTestScene.unity",
            "Assets/Tests/PlayModeTests/MoveToPositionJumpThroughObstacle.unity",
            "Assets/Tests/PlayModeTests/MoveToPositionOneWayJumpWithJumpAsLastSegmentTestScene.unity",
            "Assets/Tests/PlayModeTests/MoveToPositionOnly1NodeTestScene.unity",
        };

        [Test]
        public void BakeScene([ValueSource(nameof(Scenes))] string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath);
            var world = Object.FindObjectOfType<NavWorld>();
            NavBaker.BakePathfinding(world);
            Snapshot.Test(world);
        }

        //[UnityTest]
        //public IEnumerator NewTestScriptWithEnumeratorPasses()
        //{
        //    EditorSceneManager.OpenScene("Assets/Tests/PlayModeTests/MoveToPositionTestScene.unity");
        //    // Use the Assert class to test conditions.
        //    // Use yield to skip a frame.

        //    var world = Object.FindObjectOfType<NavWorld>();
        //    NavBaker.BakePathfinding(world);
        //    yield return null;
        //}
    }
}