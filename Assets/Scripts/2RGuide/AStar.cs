using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts._2RGuide
{
    public enum ConnectionType
    {
        Walk,
        Drop,
        Jump
    }

    [Serializable]
    public struct NodeConnection
    {
        public Node node;
        public ConnectionType connectionType;
        public LineSegment2D segment;

        public static NodeConnection Walk(Node node, LineSegment2D segment)
        {
            return new NodeConnection { node = node, connectionType = ConnectionType.Walk, segment = segment };
        }
        public static NodeConnection Drop(Node node, LineSegment2D segment)
        {
            return new NodeConnection { node = node, connectionType = ConnectionType.Drop, segment = segment };
        }
        public static NodeConnection Jump(Node node, LineSegment2D segment)
        {
            return new NodeConnection { node = node, connectionType = ConnectionType.Jump, segment = segment };
        }
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
        public List<NodeConnection> Connections => _connections;

        public Node()
        {
            _connections = new List<NodeConnection>();
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

        //public static bool operator ==(Node node1, Node node2)
        //{
        //    if(node1 == null && node2 == null)
        //    {
        //        return true;
        //    }
        //    if (node1 == null || node2 == null)
        //    {
        //        return false;
        //    }

        //    return node1.Position.Approximately(node2.Position);
        //}

        //public static bool operator !=(Node node1, Node node2)
        //{
        //    return !(node1 == node2);
        //}
    }

    public class AStar
    {
        public Node[] Resolve(Node start, Node goal)
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

        private Node[] ReconstructPath(Dictionary<Node, Node> cameFrom, Node current, Node goal)
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


        private float Heuristic(Node node, Node goal)
        {
            return Vector2.Distance(node.Position, goal.Position);
        }
    }
}