﻿using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Helpers;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    public interface IAgentOperationsContext
    {
        Vector2 Position { get; }
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
        TaskCoroutine<GuideAgentHelper.PathfindingResult> FindPath(
            Vector2 start,
            Vector2 end,
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