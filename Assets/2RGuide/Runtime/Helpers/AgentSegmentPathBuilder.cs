using System.Linq;
using UnityEngine;
using static Assets._2RGuide.Runtime.GuideAgent;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class AgentSegmentPathBuilder
    {
        public static AgentSegment[] BuildPathFrom(Vector2 startPosition, Vector2 targetPosition, Node[] path, float segmentProximityMaxDistance, float maxSlopeDegrees)
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

            var segmentPath = agentSegmentPath.ToList();

            // if character is already in between first and second node no need to go back to first
            if (path.Count() > 1 && path[0].ConnectionWith(path[1]).Value.ConnectionType == ConnectionType.Walk)
            {
                var closestPositionWithStart = path[0].ConnectionWith(path[1]).Value.Segment.ClosestPointOnLine(startPosition);
                var firstElement = segmentPath[0];
                firstElement.position = closestPositionWithStart;
                segmentPath[0] = firstElement;
            }

            var lastConnection = path[path.Length - 2].ConnectionWith(path.Last()).Value;
            // if character doesn't want to move to last node it should stay "half way"
            if (path.Count() > 1 && lastConnection.ConnectionType == ConnectionType.Walk)
            {
                //ToDo: first check if on segment between length-2 and length-1, if yes assign closest to last position,
                //otherwise check connections for last node for closest value on segment
                var closestPositionWithTarget = lastConnection.Segment.ClosestPointOnLine(targetPosition);
                if (!closestPositionWithTarget.Approximately(lastConnection.Node.Position) && lastConnection.Segment.OnSegment(closestPositionWithTarget))
                {
                    var lastElement = segmentPath.Last();
                    lastElement.position = closestPositionWithTarget;
                    segmentPath[segmentPath.Count - 1] = lastElement;
                }
                else
                {
                    var lastNode = path.Last();
                    // is segment eligible
                    var eligibleConnections = lastNode.Connections.Where(c =>
                    {
                        if (c.ConnectionType != ConnectionType.Walk)
                        {
                            return false;
                        }
                        if (c.Segment.IsCoincident(lastConnection.Segment))
                        {
                            return false;
                        }
                        if (c.Segment.SlopeRadians > (maxSlopeDegrees * Mathf.Deg2Rad))
                        {
                            return false;
                        }

                        var closestPoint = c.Segment.ClosestPointOnLine(targetPosition);
                        return Vector2.Distance(closestPoint, targetPosition) < segmentProximityMaxDistance;
                    });

                    if (eligibleConnections.Any())
                    {
                        var closestConnection = eligibleConnections.MinBy(c =>
                        {
                            var closestPoint = c.Segment.ClosestPointOnLine(targetPosition);
                            return Vector2.Distance(closestPoint, targetPosition);
                        });

                        segmentPath.Add(new AgentSegment()
                        {
                            connectionType = ConnectionType.Walk,
                            position = closestConnection.Segment.ClosestPointOnLine(targetPosition)
                        });
                    }
                }
            }

            var distinctedSegmentPath = segmentPath.Distinct();
            return distinctedSegmentPath.ToArray();
        }
    }
}