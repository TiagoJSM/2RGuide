using _2RGuide;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using static _2RGuide.GuideAgent;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class AgentSegmentPathBuilder
    {
        public static AgentSegment[] BuildPathFrom(Vector2 startPosition, Vector2 targetPosition, Node[] path)
        {
            var agentSegmentPath =
                path
                    .Select((n, i) =>
                    {
                        var connectionType =
                            i == 0
                            ? ConnectionType.Walk
                            : path[i - 1].ConnectionWith(path[i]).Value.ConnectionType;
                        return new AgentSegment() { position = n.Position, connectionType = connectionType };
                    });

            var segmentPath = agentSegmentPath.ToArray();

            // if character is already in between first and second node no need to go back to first
            if (path.Count() > 1)
            {
                var closestPositionWithStart = path[0].ConnectionWith(path[1]).Value.Segment.ClosestPointOnLine(startPosition);
                segmentPath[0].position = closestPositionWithStart;
            }

            // if character doesn't want to move to last node it should stay "half way"
            if (path.Count() > 1)
            {
                //ToDo: first check if on segment between length-2 and length-1, if yes run code bellow, otherwise check connections for last node for closest value on segment
                var closestPositionWithTarget = path[path.Length - 2].ConnectionWith(path.Last()).Value.Segment.ClosestPointOnLine(targetPosition);
                segmentPath[segmentPath.Length - 1].position = closestPositionWithTarget;
            }

            var distinctedSegmentPath = segmentPath.Distinct();
            return distinctedSegmentPath.ToArray();
        }
    }
}