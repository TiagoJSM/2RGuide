using Assets._2RGuide.Runtime.Math;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using static Assets._2RGuide.Runtime.GuideAgent;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class AgentSegmentPathBuilder
    {
        public static AgentSegment[] BuildPathFrom(Vector2 startPosition, Vector2 targetPosition, Node[] path, float segmentProximityMaxDistance, float maxSlopeDegrees)
        {
            var agentSegments = new List<AgentSegment>();

            for (var nodeIndex = 0; nodeIndex < path.Length; nodeIndex++)
            {
                var connectionType =
                    nodeIndex == 0
                    ? ConnectionType.Walk
                    : path[nodeIndex - 1].ConnectionWith(path[nodeIndex]).Value.ConnectionType;

                agentSegments.Add(new AgentSegment() { position = path[nodeIndex].Position, connectionType = connectionType });
            }

            var firstWalkableConnection = path[0].GetWalkableConnectionForPosition(startPosition, segmentProximityMaxDistance, maxSlopeDegrees);
            var firstClosestPoint = firstWalkableConnection.Value.Segment.ClosestPointOnLine(startPosition);

            if(path.Length > 1 && IsPositionInBetweenConnection(path, 0, firstClosestPoint, segmentProximityMaxDistance))
            {
                var agentSegment = agentSegments[0];
                agentSegment.position = firstClosestPoint;
                agentSegments[0] = agentSegment;
            }
            else
            {
                agentSegments.Insert(0, new AgentSegment() { position = firstClosestPoint, connectionType = ConnectionType.Walk });
            }

            var lastWalkableConnection = path.Last().GetWalkableConnectionForPosition(targetPosition, float.MaxValue, maxSlopeDegrees);
            var lastClosestPoint = lastWalkableConnection.Value.Segment.ClosestPointOnLine(targetPosition);

            var lastSegment = new LineSegment2D(agentSegments[agentSegments.Count - 2].position, agentSegments[agentSegments.Count - 1].position);
            
            if (lastSegment.Contains(lastClosestPoint))
            {
                var agentSegment = agentSegments[agentSegments.Count - 1];
                agentSegment.position = lastClosestPoint;
                agentSegments[agentSegments.Count - 1] = agentSegment;
            }
            else
            {
                agentSegments.Add(new AgentSegment() { position = lastClosestPoint, connectionType = ConnectionType.Walk });
            }

            return agentSegments.DistinctBy(s => s.position).ToArray();
        }
        /*public static AgentSegment[] BuildPathFrom(Vector2 startPosition, Vector2 targetPosition, Node[] path, float segmentProximityMaxDistance, float maxSlopeDegrees)
        {
            var agentSegments = new List<AgentSegment>();

            for (var nodeIndex = 0; nodeIndex < path.Length; nodeIndex++)
            {
                if (nodeIndex == 0)
                {
                    var closestWalkableConnection = path[0].GetWalkableConnectionForPosition(startPosition, segmentProximityMaxDistance, maxSlopeDegrees);
                    if (closestWalkableConnection != null)
                    {
                        // Get middle point from closest segment
                        var closestPoint = closestWalkableConnection.Value.Segment.ClosestPointOnLine(startPosition);
                        agentSegments.Add(new AgentSegment() { position = closestPoint, connectionType = ConnectionType.Walk });

                        if (path.Length > 1)
                        {
                            var segment = path[0].ConnectionWith(path[1]).Value.Segment;
                            // if segment is not in between 2 starting nodes connect first node
                            if (!segment.IsCoincident(closestWalkableConnection.Value.Segment))
                            {
                                agentSegments.Add(new AgentSegment() { position = path[0].Position, connectionType = ConnectionType.Walk });
                            }
                        }
                    }
                    else // if there's nothing close by just connection first node
                    {
                        agentSegments.Add(new AgentSegment() { position = path[0].Position, connectionType = ConnectionType.Walk });
                    }

                    continue;
                }

                if(nodeIndex == path.Length - 1)
                {
                    var closestWalkableConnection = path[nodeIndex].GetWalkableConnectionForPosition(targetPosition, segmentProximityMaxDistance, maxSlopeDegrees);
                    if (closestWalkableConnection != null && path.Length > 1)
                    {
                        var segment = path[nodeIndex - 1].ConnectionWith(path[nodeIndex]).Value.Segment;
                        var closestPoint = closestWalkableConnection.Value.Segment.ClosestPointOnLine(targetPosition);
                        if (segment.IsCoincident(closestWalkableConnection.Value.Segment))
                        {
                            agentSegments.Add(new AgentSegment() { position = closestPoint, connectionType = ConnectionType.Walk });
                        }
                        else
                        {
                            agentSegments.Add(new AgentSegment() { position = path[nodeIndex].Position, connectionType = ConnectionType.Walk });
                            agentSegments.Add(new AgentSegment() { position = closestPoint, connectionType = ConnectionType.Walk });
                        }
                    }
                    else
                    {
                        agentSegments.Add(new AgentSegment() { position = path[nodeIndex].Position, connectionType = ConnectionType.Walk });
                    }
                    continue;
                }

                var connectionType =
                    nodeIndex == 0
                    ? ConnectionType.Walk
                    : path[nodeIndex - 1].ConnectionWith(path[nodeIndex]).Value.ConnectionType;

                agentSegments.Add(new AgentSegment() { position = path[nodeIndex].Position, connectionType = connectionType });
            }

            return agentSegments.ToArray();
        }*/

        private static bool IsPositionInBetweenConnection(Node[] path, int index, Vector2 position, float segmentProximityMaxDistance)
        {
            var segment = path[index].ConnectionWith(path[index + 1]).Value.Segment;
            var closestPoint = segment.ClosestPointOnLine(position);
            return Vector2.Distance(position, closestPoint) < segmentProximityMaxDistance;
        }

        /*public static AgentSegment[] BuildPathFrom(Vector2 startPosition, Vector2 targetPosition, Node[] path, float segmentProximityMaxDistance, float maxSlopeDegrees)
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

            
            // if character doesn't want to move to last node it should stay "half way"
            if (path.Count() > 1)
            {
                var lastConnection = path[path.Length - 2].ConnectionWith(path.Last()).Value;
                //ToDo: first check if on segment between length-2 and length-1, if yes assign closest to last position,
                //otherwise check connections for last node for closest value on segment
                var closestPositionWithTarget = lastConnection.Segment.ClosestPointOnLine(targetPosition);
                if (!closestPositionWithTarget.Approximately(lastConnection.Node.Position) && lastConnection.Segment.OnSegment(closestPositionWithTarget) && lastConnection.ConnectionType == ConnectionType.Walk)
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
        }*/
    }
}