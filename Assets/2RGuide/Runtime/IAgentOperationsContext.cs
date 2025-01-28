using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    public interface IAgentOperationsContext
    {
        Vector2 Position { get; }
        NavWorldManager NavWorldManager { get; }
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
        TaskCoroutine<PathfindingTask.PathfindingResult> RunPathfinding(
            RGuideVector2 start,
            RGuideVector2 end,
            float maxHeight,
            float maxSlopeDegrees,
            ConnectionType allowedConnectionTypes,
            float pathfindingMaxDistance,
            float segmentProximityMaxDistance,
            NavTag[] navTagCapable,
            float stepHeight,
            ConnectionTypeMultipliers connectionMultipliers);
    }
}