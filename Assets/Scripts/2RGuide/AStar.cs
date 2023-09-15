using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts._2RGuide
{
    public enum ConnectionType
    {
        Walk,
        Drop,
        Jump,
        OneWayPlatformJump
    }

    [Serializable]
    public struct NodeConnection
    {
        public Node node;
        public ConnectionType connectionType;
        public LineSegment2D segment;
        public float maxHeight;
    }

    [Serializable]
    public class Node
    {
        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private List<NodeConnection> _connections;

        public Vector2 Position 
        {
            get => _position;
            set => _position = value;
        }
        public IEnumerable<NodeConnection> Connections => _connections;

        public Node()
        {
            _connections = new List<NodeConnection>();
        }

        public bool AddConnection(ConnectionType connectionType, Node other, LineSegment2D segment, float maxHeight)
        {
            var hasSegment = _connections.Any(c => c.segment.IsCoincident(segment));
            if (!hasSegment)
            {
                _connections.Add(new NodeConnection { node = other, connectionType = connectionType, segment = segment, maxHeight = maxHeight });
            }

            return !hasSegment;
        }

        public NodeConnection? ConnectionWith(Node n)
        {
            var nc = Connections.FirstOrDefault(c => c.node.Equals(n));
            return nc;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is Node other)
            {
                return other.Position.Approximately(Position);
            }
            return false;
        }
    }

    public static class AStar
    {
        public static Node[] Resolve(Node start, Node goal, float maxHeight, float maxSlope)
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
                    if(neighbor.maxHeight < maxHeight)
                    {
                        continue;
                    }
                    if (neighbor.segment.SlopeRadians > maxSlope)
                    {
                        continue;
                    }

                    var tentativeGScore = gScore[current] + Vector2.Distance(current.Position, neighbor.node.Position);
                    if (tentativeGScore < gScore.GetValueOrDefault(neighbor.node, float.PositiveInfinity))
                    {
                        cameFrom[neighbor.node] = current;
                        gScore[neighbor.node] = tentativeGScore;
                        var currentFScore = tentativeGScore + Heuristic(neighbor.node, goal);
                        fScore[neighbor.node] = currentFScore;
                        if (!queue.Contains(neighbor.node))
                        {
                            queue.Enqueue(neighbor.node, currentFScore);
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