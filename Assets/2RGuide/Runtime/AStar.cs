using _2RGuide.Helpers;
using _2RGuide.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide
{
    public static class AStar
    {
        public static Node[] Resolve(Node start, Node goal, float maxHeight, float maxSlope, ConnectionType allowedConnectionTypes)
        {
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
                    return ReconstructPath(cameFrom, current, goal);
                }

                foreach (var neighbor in current.Connections)
                {
                    if(neighbor.MaxHeight < maxHeight)
                    {
                        continue;
                    }
                    if (neighbor.Segment.SlopeRadians > maxSlope)
                    {
                        continue;
                    }
                    if (!allowedConnectionTypes.HasFlag(neighbor.ConnectionType))
                    {
                        continue;
                    }

                    var tentativeGScore = gScore[current] + Vector2.Distance(current.Position, neighbor.Node.Position);
                    if (tentativeGScore < gScore.GetValueOrDefault(neighbor.Node, float.PositiveInfinity))
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

            return null;
        }

        private static Node[] ReconstructPath(Dictionary<Node, Node> cameFrom, Node current, Node goal)
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
            return Vector2.Distance(node.Position, goal.Position);
        }
    }
}