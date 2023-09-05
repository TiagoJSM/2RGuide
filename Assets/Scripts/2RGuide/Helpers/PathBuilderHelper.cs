using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class PathBuilderHelper
    {
        public static void AddTargetNodeForSegment(LineSegment2D target, List<Node> nodes, LineSegment2D[] segments, Node startNode, ConnectionType connectionType)
        {
            var existingNodeAtPosition = nodes.FirstOrDefault(n => n.Position == target.P2);

            if (existingNodeAtPosition != null)
            {
                return;
            }

            var dropTargetSegment = segments.FirstOrDefault(s => s.OnSegment(target.P2));

            if (!dropTargetSegment)
            {
                return;
            }

            var targetNode = new Node() { Position = target.P2 };

            var connectedNode1 = nodes.FirstOrDefault(n => n.Position == dropTargetSegment.P1);
            if (connectedNode1 != null)
            {
                targetNode.Connections.Add(NodeConnection.Walk(connectedNode1, new LineSegment2D(dropTargetSegment.P1, targetNode.Position)));
            }

            var connectedNode2 = nodes.FirstOrDefault(n => n.Position == dropTargetSegment.P2);
            if (connectedNode2 != null)
            {
                targetNode.Connections.Add(NodeConnection.Walk(connectedNode2, new LineSegment2D(targetNode.Position, dropTargetSegment.P2)));
            }

            AddConnection(startNode, targetNode, connectionType);
            nodes.Add(targetNode);
        }

        private static void AddConnection(Node startNode, Node endNode, ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case ConnectionType.Walk:
                    startNode.Connections.Add(NodeConnection.Walk(endNode, new LineSegment2D(startNode.Position, endNode.Position)));
                    break;
                case ConnectionType.Drop:
                    startNode.Connections.Add(NodeConnection.Drop(endNode, new LineSegment2D(startNode.Position, endNode.Position)));
                    break;
                case ConnectionType.Jump:
                    startNode.Connections.Add(NodeConnection.Jump(endNode, new LineSegment2D(startNode.Position, endNode.Position)));
                    endNode.Connections.Add(NodeConnection.Jump(startNode, new LineSegment2D(endNode.Position, startNode.Position)));
                    break;
            }
        }
    }
}