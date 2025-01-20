using Assets._2RGuide.Runtime;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets._2RGuide.Runtime.GuideAgent;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools.Utils;
using UnityEngine.TestTools;
using UnityEngine;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;

namespace Assets.Tests.PlayModeTests
{
    public class GuideAgentHelperTests
    {
        public class PathfindingTaskParams
        {
            public string Scene { get; }
            public string Target { get; }
            public int NumberOfNodes { get; }

            public PathfindingTaskParams(string scene, string target, int numberOfNodes)
            {
                Scene = scene;
                Target = target;
                NumberOfNodes = numberOfNodes;
            }
        }

        static readonly PathfindingTaskParams[] PathfindingTaskTestValues = new[]
        {
            new PathfindingTaskParams("MoveDownToPositionRotatedOneWayStepTestScene", "Target", 5),
        };

        [UnityTest]
        public IEnumerator PathfindingTask([ValueSource(nameof(PathfindingTaskTestValues))] PathfindingTaskParams values)
        {
            TestSceneManager.LoadScene(values.Scene);
            yield return null;

            var agentGO = GameObject.Find("Agent");
            
            Assert.That(agentGO, Is.Not.Null);
            var agent = agentGO.GetComponent<GuideAgent>();
            Assert.That(agent, Is.Not.Null);

            var targetGO = GameObject.Find(values.Target);
            Assert.That(targetGO, Is.Not.Null);

            var settings = Nav2RGuideSettings.Load();
            var navWorld = NavWorldManager.Instance.NavWorld;

            var result = _2RGuide.Runtime.Helpers.PathfindingTask.Run(
                navWorld,
                new RGuideVector2(agentGO.transform.position),
                new RGuideVector2(targetGO.transform.position), 
                agent.Height, 
                agent.MaxSlopeDegrees, 
                agent.AllowedConnectionTypes, 
                agent.PathfindingMaxDistance, 
                settings.SegmentProximityMaxDistance, 
                agent.NavTagCapable, 
                agent.StepHeight, 
                agent.ConnectionMultipliers);

            Assert.AreEqual(result.segmentPath.Length, values.NumberOfNodes);
        }
    }
}
