using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide
{
    public class NodeStore
    {
        private List<Node> _nodes = new List<Node>();

        public Node NewNode(Vector2 position)
        {
            if (!Contains(position))
            {
                var node = new Node() { Position = position };
                _nodes.Add(node);
                return node;
            }
            return null;
        }

        public Node NewNodeOrExisting(Vector2 position)
        {
            return NewNode(position) ?? Get(position);
        }

        public Node Get(Vector2 position)
        {
            return _nodes.FirstOrDefault(n => n.Position == position);
        }

        public bool Contains(Vector2 position)
        {
            return _nodes.Any(n => n.Position.Approximately(position));
        }

        public Node[] ToArray()
        {
            return _nodes.ToArray();
        }

        public void ConnectWithNodesAtSegment(Node node, LineSegment2D segment, ConnectionType connectionType = ConnectionType.Walk)
        {
            var oneWayPlatN1 = Get(segment.P1);
            var oneWayPlatN2 = Get(segment.P2);

            var connection = oneWayPlatN1.ConnectionWith(oneWayPlatN2).Value;

            var s1 = new LineSegment2D(oneWayPlatN1.Position, node.Position);
            var s2 = new LineSegment2D(node.Position, oneWayPlatN2.Position);

            node.AddConnection(connectionType, oneWayPlatN1, s1, connection.maxHeight);
            node.AddConnection(connectionType, oneWayPlatN2, s2, connection.maxHeight);

            oneWayPlatN1.AddConnection(connectionType, node, s1, connection.maxHeight);
            oneWayPlatN2.AddConnection(connectionType, node, s2, connection.maxHeight);
        }

        public LineSegment2D ConnectNodes(Node node1, Node node2, float maxHeight, ConnectionType connectionType = ConnectionType.Walk)
        {
            var s = new LineSegment2D(node1.Position, node2.Position);

            node1.AddConnection(connectionType, node2, s, maxHeight);
            node2.AddConnection(connectionType, node1, s, maxHeight);

            return s;
        }
    }
}