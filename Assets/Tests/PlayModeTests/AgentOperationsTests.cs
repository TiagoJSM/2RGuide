using Assets._2RGuide.Runtime;
using System.Collections;
using UnityEngine;
using static Assets._2RGuide.Runtime.GuideAgent;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Helpers;
using NUnit.Framework;
using System.Threading.Tasks;
using Assets._2RGuide.Runtime.Math;

namespace Assets.Tests.PlayModeTests
{
    public class AgentOperationsTests
    {
        private class TestAgentOperationsContext : IAgentOperationsContext
        {
            private MonoBehaviour _contextObject;
            private TaskCompletionSource<GuideAgentHelper.PathfindingResult> _currentTaskCompletionSource;

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

            public TaskCoroutine<GuideAgentHelper.PathfindingResult> FindPath(RGuideVector2 start, RGuideVector2 end, float maxHeight, float maxSlopeDegrees, ConnectionType allowedConnectionTypes, float pathfindingMaxDistance, float segmentProximityMaxDistance, NavTag[] navTagCapable, float stepHeight, ConnectionTypeMultipliers connectionMultipliers)
            {
                _currentTaskCompletionSource = new TaskCompletionSource<GuideAgentHelper.PathfindingResult>();
                return TaskCoroutine<GuideAgentHelper.PathfindingResult>.Run(_currentTaskCompletionSource.Task);
            }

            public void SetFindPathfindingResult(GuideAgentHelper.PathfindingResult result)
            {
                _currentTaskCompletionSource.SetResult(result);
                _currentTaskCompletionSource = null;
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

            var settings = Nav2RGuideSettings.Load();
            var speed = 1.0f;
            var height = 0.0f;
            var maxSlopeDegrees = 90.0f;
            var baseOffset = 0.0f;
            var proximityThreshold = 0.0f;
            var connectionType = ConnectionType.Walk;
            var pathfindingMaxDistance = float.PositiveInfinity;
            var navTagCapable = new NavTag[0];
            var stepHeight = 0.0f;
            var connectionMultipliers = new ConnectionTypeMultipliers();

            var context = new TestAgentOperationsContext();
            var agentOperations = new AgentOperations(context, settings, speed, height, maxSlopeDegrees, baseOffset, proximityThreshold, connectionType, pathfindingMaxDistance, navTagCapable, stepHeight, connectionMultipliers);

            var target = GameObject.Find("Target");
            var position1 = GameObject.Find("Position1");
            var position2 = GameObject.Find("Position2");

            context.Position = target.transform.position;
            target.transform.position = position1.transform.position;

            agentOperations.SetDestination(target, true);
            agentOperations.Update();
            Assert.That(agentOperations.Status == AgentStatus.Busy);
            Assert.That(agentOperations.IsSearchingForPath);

            var pathfindingRoutine = RunPathfindingRoutine(new RGuideVector2(context.Position), new RGuideVector2(target.transform.position), settings, height, maxSlopeDegrees, connectionType, pathfindingMaxDistance, navTagCapable, stepHeight, connectionMultipliers);
            yield return pathfindingRoutine;
            
            context.SetFindPathfindingResult(pathfindingRoutine.Result);

            yield return null;

            MoveContextAlongPath(context, agentOperations, target, position2);

            pathfindingRoutine = RunPathfindingRoutine(new RGuideVector2(context.Position), new RGuideVector2(target.transform.position), settings, height, maxSlopeDegrees, connectionType, pathfindingMaxDistance, navTagCapable, stepHeight, connectionMultipliers);
            yield return pathfindingRoutine;

            context.SetFindPathfindingResult(pathfindingRoutine.Result);

            yield return null;

            MoveContextAlongPath(context, agentOperations, target, null);

            Assert.That(agentOperations.Status == AgentStatus.Iddle);
            Assert.That(!agentOperations.IsSearchingForPath);
        }

        private TaskCoroutine<GuideAgentHelper.PathfindingResult> RunPathfindingRoutine(
            RGuideVector2 start,
            RGuideVector2 end,
            Nav2RGuideSettings settings,
            float height,
            float maxSlopeDegrees,
            ConnectionType allowedConnectionTypes,
            float pathfindingMaxDistance,
            NavTag[] navTagCapable,
            float stepHeight,
            ConnectionTypeMultipliers connectionMultipliers)
        {
            return TaskCoroutine<GuideAgentHelper.PathfindingResult>.Run(() =>
                GuideAgentHelper.PathfindingTask(
                        start,
                        end,
                        height,
                        maxSlopeDegrees,
                        allowedConnectionTypes,
                        pathfindingMaxDistance,
                        settings.SegmentProximityMaxDistance,
                        navTagCapable,
                        stepHeight,
                        connectionMultipliers));
        }

        private void MoveContextAlongPath(TestAgentOperationsContext context, AgentOperations agentOperations, GameObject target, GameObject nextTargetPosition)
        {
            var path = agentOperations.Path;
            while(agentOperations.TargetPathIndex < path.Length)
            {
                context.Position = path[agentOperations.TargetPathIndex].Position.ToVector2();
                if (agentOperations.TargetPathIndex == (path.Length - 1) && nextTargetPosition != null)
                {
                    target.transform.position = nextTargetPosition.transform.position;
                }
                agentOperations.Update();
            }

            if(nextTargetPosition != null)
            {
                Assert.That(agentOperations.Status == AgentStatus.Busy);
                Assert.That(agentOperations.IsSearchingForPath);
            }
        }
    }
}