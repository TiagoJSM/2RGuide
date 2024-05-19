using Assets._2RGuide.Runtime;
using Assets.Tests.PlayModeTests.Attributes;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using static Assets._2RGuide.Runtime.GuideAgent;

namespace Assets.Tests.PlayModeTests
{
    public class AgentTests
    {
        public class AgentTargetMovementParams
        {
            public string Scene { get; set; }
            public string[] Targets { get; set; }

            public AgentTargetMovementParams(string scene, string[] targets)
            {
                Scene = scene;
                Targets = targets;
            }
        }

        static AgentTargetMovementParams[] MoveToTargetsTestValues = new[]
        {
            new AgentTargetMovementParams("MoveToPositionTestScene", new[] { "Target" }),
            new AgentTargetMovementParams("MoveToPositionWithJumpsTestScene", new[] { "Target" }),
            new AgentTargetMovementParams("MoveToPositionOneWayJumpTestScene", new[] { "Target" }),
            new AgentTargetMovementParams("MoveToAdjacentSegmentTestScene", new[] { "Target" }),
            new AgentTargetMovementParams("MoveToAdjacentSegmentButCloserNodeTestScene", new[] { "Target" }),
            new AgentTargetMovementParams("MoveToPositionThroughObstacleTestScene", new[] { "Target" }),
            new AgentTargetMovementParams("MoveToPositionJumpThroughObstacle", new[] { "Target" }),
            new AgentTargetMovementParams("MoveToPositionOneWayJumpWithJumpAsLastSegmentTestScene", new[] { "Target" })
        };

        [UnityTest]
        public IEnumerator MoveToTargetsTest([ValueSource(nameof(MoveToTargetsTestValues))] AgentTargetMovementParams values)
        {
            Debug.Log($"Test running on scene {values.Scene}");
            SceneManager.LoadScene(values.Scene);
            yield return null;

            var agentGO = GameObject.Find("Agent");
            Assert.That(agentGO, Is.Not.Null);
            var agent = agentGO.GetComponent<TransformMovement>();
            Assert.That(agent, Is.Not.Null);

            var targetGOs = values.Targets.Select(t => GameObject.Find(t));

            foreach (var targetGO in targetGOs)
            {
                Assert.That(targetGO, Is.Not.Null);
                Assert.AreNotEqual(targetGO.transform.position, agent.transform.position);
            }

            var comparer = new Vector3EqualityComparer(0.25f);

            foreach (var targetGO in targetGOs)
            {
                agent.Target = targetGO.transform;
                yield return new WaitForSeconds(1.0f);
                Assert.That(agent.GuideAgent.CurrentPathStatus == PathStatus.Complete);
                Assert.That(agent.transform.position, Is.EqualTo(targetGO.transform.position).Using(comparer));
            }
        }

        [UnityTest, TestScene("MoveToNonReachablePositionTestScene")]
        public IEnumerator VerifyAgentCantReachGoalButMovesToClosestPosition()
        {
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            var comparer = new Vector3EqualityComparer(0.25f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(1.565f, -3.45f)).Using(comparer));
            yield return new WaitForSeconds(1.0f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(6.62f, -3.615f)).Using(comparer));
        }

        [UnityTest, TestScene("MoveToNonReachablePositionRepeatPathfindingTestScene")]
        public IEnumerator VerifyAgentCantReachGoalButMovesToClosestPositionAfterRepeatedPathfinding()
        {
            var agent = GameObject.Find("Agent").GetComponent<TransformMovement>();
            var target = GameObject.Find("Target");
            Assert.That(agent, Is.Not.Null);
            var comparer = new Vector3EqualityComparer(0.25f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(1.565f, -3.45f)).Using(comparer));
            agent.Target = target.transform;
            yield return new WaitForSeconds(1.0f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(6.62f, -3.615f)).Using(comparer));
            agent.Target = target.transform;
            yield return new WaitForSeconds(1.0f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(6.62f, -3.615f)).Using(comparer));
        }

        [UnityTest, TestScene("MoveToPositionPartiallyTestScene")]
        public IEnumerator VerifyAgentCanMovePartiallyCloseToTarget()
        {
            var agent = GameObject.Find("Agent").GetComponent<TransformMovement>();
            var guide = agent.GuideAgent;
            var target = GameObject.Find("Target");
            var finalPos = GameObject.Find("FinalPosition");
            Assert.That(agent, Is.Not.Null);
            var comparer = new Vector3EqualityComparer(0.25f);
            agent.Target = target.transform;
            yield return new WaitForSeconds(1.0f);
            Assert.IsTrue(guide.CurrentPathStatus == PathStatus.Incomplete);
            Assert.That(agent.transform.position, Is.EqualTo(finalPos.transform.position).Using(comparer));
        }
    }
}