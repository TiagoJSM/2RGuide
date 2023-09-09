using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class PathBuilderHelper
    {
        public static void AddTargetNodeForSegment(LineSegment2D target, List<Node> nodes, LineSegment2D[] segments, Node startNode, ConnectionType connectionType, float maxSlope)
        {
            var targetNode = nodes.FirstOrDefault(n => n.Position == target.P2);

            if (targetNode == null)
            {
                targetNode = new Node() { Position = target.P2 };

                var dropTargetSegment = segments.FirstOrDefault(s => !s.OverMaxSlope(maxSlope) && s.OnSegment(target.P2));

                if (!dropTargetSegment)
                {
                    return;
                }

                var connectedNode1 = nodes.FirstOrDefault(n => n.Position == dropTargetSegment.P1);
                if (connectedNode1 != null)
                {
                    targetNode.AddConnection(ConnectionType.Walk, connectedNode1, new LineSegment2D(dropTargetSegment.P1, targetNode.Position));
                    connectedNode1.AddConnection(ConnectionType.Walk, targetNode, new LineSegment2D(targetNode.Position, dropTargetSegment.P1));
                }

                var connectedNode2 = nodes.FirstOrDefault(n => n.Position == dropTargetSegment.P2);
                if (connectedNode2 != null)
                {
                    targetNode.AddConnection(ConnectionType.Walk, connectedNode2, new LineSegment2D(targetNode.Position, dropTargetSegment.P2));
                    connectedNode2.AddConnection(ConnectionType.Walk, targetNode, new LineSegment2D(dropTargetSegment.P2, targetNode.Position));
                }
            }

            AddConnection(startNode, targetNode, connectionType);
            nodes.Add(targetNode);
        }

        private static void AddConnection(Node startNode, Node endNode, ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case ConnectionType.Walk:
                    startNode.AddConnection(ConnectionType.Walk, endNode, new LineSegment2D(startNode.Position, endNode.Position));
                    break;
                case ConnectionType.Drop:
                    startNode.AddConnection(ConnectionType.Drop, endNode, new LineSegment2D(startNode.Position, endNode.Position));
                    break;
                case ConnectionType.Jump:
                    startNode.AddConnection(ConnectionType.Jump, endNode, new LineSegment2D(startNode.Position, endNode.Position));
                    endNode.AddConnection(ConnectionType.Jump, startNode, new LineSegment2D(endNode.Position, startNode.Position));
                    break;
            }
        }
    }
}