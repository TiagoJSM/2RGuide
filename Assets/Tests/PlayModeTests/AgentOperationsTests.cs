using Assets._2RGuide.Runtime;
using System.Collections;
using System.Linq;
using UnityEngine;
using static Assets._2RGuide.Runtime.GuideAgent;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools.Utils;
using UnityEngine.TestTools;
using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Helpers;
using NUnit.Framework;
using UnityEditor;
using System.Threading;
using UnityEngine.UIElements;

namespace Assets.Tests.PlayModeTests
{
    public class AgentOperationsTests
    {
        private class TestAgentOperationsContext : IAgentOperationsContext
        {
            private MonoBehaviour _contextObject;
            public TestAgentOperationsContext()
            {
                var go = new GameObject("Context Object", new System.Type[] { typeof(Dummy) });
                _contextObject = go.GetComponent<Dummy>();
            }

            public Vector2 Position
            {
                get
                {
                    return _contextObject.transform.position;
                }
                set
                {
                    _contextObject.transform.position = value;
                }
            }

            public TaskCoroutine<GuideAgentHelper.PathfindingResult> FindPath(Vector2 start, Vector2 end, float maxHeight, float maxSlopeDegrees, ConnectionType allowedConnectionTypes, float pathfindingMaxDistance, float segmentProximityMaxDistance, NavTag[] navTagCapable, float stepHeight, ConnectionTypeMultipliers connectionMultipliers)
            {
                return TaskCoroutine<GuideAgentHelper.PathfindingResult>.Run(() =>
                {
                    Thread.Sleep(3000);
                    return GuideAgentHelper.PathfindingTask(
                        start,
                        end,
                        maxHeight,
                        maxSlopeDegrees,
                        allowedConnectionTypes,
                        pathfindingMaxDistance,
                        segmentProximityMaxDistance,
                        navTagCapable,
                        stepHeight,
                        connectionMultipliers);
                });
            }

            public Coroutine StartCoroutine(IEnumerator routine)
            {
                return _contextObject.StartCoroutine(routine);
            }

            public void StopCoroutine(Coroutine routine)
            {
                _contextObject.StopCoroutine(routine);
            }
        }

        [UnityTest]
        public IEnumerator MoveToTargetsTest()
        {
            Debug.Log($"Test running on scene FollowMovingTargetTestScene");
            SceneManager.LoadScene("FollowMovingTargetTestScene");
            yield return null;

            var context = new TestAgentOperationsContext();
            var agentOperations = new AgentOperations(context, Nav2RGuideSettings.Load(), 1.0f, 0, 90.0f, 0, 0, ConnectionType.Walk, float.PositiveInfinity, new NavTag[0], 0, new ConnectionTypeMultipliers());

            var target = GameObject.Find("Target");
            var position1 = GameObject.Find("Position1");
            var position2 = GameObject.Find("Position2");

            agentOperations.SetDestination(target);
            agentOperations.Update();
            Assert.That(agentOperations.Status == AgentStatus.Busy);
            yield return new WaitWhile(() => agentOperations.IsSearchingForPath);

            MoveContextAlongPath(context, agentOperations, target, position2);

            Assert.That(agentOperations.Status == AgentStatus.Busy);
            //yield return new WaitWhile(() => agentOperations.IsSearchingForPath);
            Assert.That(agentOperations.CurrentPathStatus == PathStatus.Complete);

            MoveContextAlongPath(context, agentOperations, target, null);

            Assert.That(agentOperations.Status == AgentStatus.Iddle);
        }

        private void MoveContextAlongPath(TestAgentOperationsContext context, AgentOperations agentOperations, GameObject target, GameObject nextTargetPosition)
        {
            var path = agentOperations.Path;
            while(agentOperations.TargetPathIndex < path.Length)
            {
                context.Position = path[agentOperations.TargetPathIndex].position;
                if (agentOperations.TargetPathIndex == (path.Length - 1) && nextTargetPosition != null)
                {
                    target.transform.position = nextTargetPosition.transform.position;
                }
                agentOperations.Update();
            }

            if(nextTargetPosition != null)
            {
                Assert.That(agentOperations.Status == AgentStatus.Busy);
            }
        }
    }
}