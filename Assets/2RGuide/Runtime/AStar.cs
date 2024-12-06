using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    public static class AStar
    {
        public static Node[] Resolve(
            Node start, 
            Node goal, 
            float maxHeight, 
            float maxSlopeDegrees, 
            ConnectionType allowedConnectionTypes, 
            float maxDistance,
            NavTag[] navTagCapable, 
            float stepHeight,
            ConnectionTypeMultipliers connectionMultipliers)
        {
            var multipliers = new Dictionary<ConnectionType, float>
            {
                { ConnectionType.Walk, connectionMultipliers.Walk },
                { ConnectionType.Drop, connectionMultipliers.Drop },
                { ConnectionType.Jump, connectionMultipliers.Jump },
                { ConnectionType.OneWayPlatformJump, connectionMultipliers.OneWayPlatformJump },
                { ConnectionType.OneWayPlatformDrop, connectionMultipliers.OneWayPlatformDrop },
            };
            var queue = new PriorityQueue<Node, float>();
            queue.Enqueue(start, 0);

            var cameFrom = new Dictionary<Node, Node>();

            var gScore = new Dictionary<Node, float> //map with default value of Infinity
            {
                { start, 0.0f }
            };

            var fScore = new Dictionary<Node, float>
            {
                { start, Heuristic(start, goal) }
            };

            while(queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Equals(goal))
                {
                    return ReconstructPath(cameFrom, current);
                }

                foreach (var neighbor in current.Connections)
                {
                    if (neighbor.NavTag && !navTagCapable.Contains(neighbor.NavTag))
                    {
                        continue;
                    }
                    if (neighbor.ConnectionType == ConnectionType.Walk && Mathf.Abs(neighbor.Segment.SlopeDegrees) > maxSlopeDegrees && !neighbor.CanWalkOnStep(stepHeight))
                    {
                        continue;
                    }
                    if (neighbor.MaxHeight < maxHeight && neighbor.IsWalkable(maxSlopeDegrees))
                    {
                        continue;
                    }
                    if (!allowedConnectionTypes.HasFlag(neighbor.ConnectionType))
                    {
                        continue;
                    }

                    var tentativeGScore = gScore[current] + (neighbor.Segment.Lenght * multipliers[neighbor.ConnectionType]);

                    if (tentativeGScore > maxDistance)
                    {
                        continue;
                    }

                    if (tentativeGScore < gScore.GetValueOrDefaultValue(neighbor.Node, float.PositiveInfinity))
                    {
                        cameFrom[neighbor.Node] = current;
                        gScore[neighbor.Node] = tentativeGScore;
                        var currentFScore = tentativeGScore + Heuristic(neighbor.Node, goal);
                        fScore[neighbor.Node] = currentFScore;
                        if (!queue.Contains(neighbor.Node))
                        {
                            queue.Enqueue(neighbor.Node, currentFScore);
                        }
                    }
                }
            }

            var kvp = cameFrom.MinBy(kvp => RGuideVector2.Distance(kvp.Key.Position, goal.Position));
            
            if (kvp.Key != null)
            {
                var closestNodeDistance = RGuideVector2.Distance(kvp.Key.Position, goal.Position);
                var startNodeDistance = RGuideVector2.Distance(start.Position, goal.Position);
                if (closestNodeDistance < startNodeDistance)
                {
                    return ReconstructPath(cameFrom, kvp.Key);
                }
                else
                {
                    return new Node[] { start };
                }
            }

            return null;
        }

        private static Node[] ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
        {
            var path = new List<Node>() { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path.ToArray();
        }


        private static float Heuristic(Node node, Node goal)
        {
            return RGuideVector2.Distance(node.Position, goal.Position);
        }
    }
}