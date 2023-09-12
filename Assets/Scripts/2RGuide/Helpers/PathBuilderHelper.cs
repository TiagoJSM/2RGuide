using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class PathBuilderHelper
    {
        public static void AddTargetNodeForSegment(LineSegment2D target, NodeStore nodeStore, NavSegment[] navSegments, Node startNode, ConnectionType connectionType, float maxSlope, float maxHeight)
        {
            var targetNode = nodeStore.Get(target.P2);

            if (targetNode == null)
            {
                targetNode = nodeStore.NewNode(target.P2);

                var dropTargetSegment = navSegments.FirstOrDefault(ss => !ss.segment.OverMaxSlope(maxSlope) && ss.segment.OnSegment(target.P2));

                if (!dropTargetSegment)
                {
                    return;
                }

                var connectedNode1 = nodeStore.Get(dropTargetSegment.segment.P1);
                if (connectedNode1 != null)
                {
                    targetNode.AddConnection(ConnectionType.Walk, connectedNode1, new LineSegment2D(dropTargetSegment.segment.P1, targetNode.Position), dropTargetSegment.maxHeight);
                    connectedNode1.AddConnection(ConnectionType.Walk, targetNode, new LineSegment2D(targetNode.Position, dropTargetSegment.segment.P1), dropTargetSegment.maxHeight);
                }

                var connectedNode2 = nodeStore.Get(dropTargetSegment.segment.P2);
                if (connectedNode2 != null)
                {
                    targetNode.AddConnection(ConnectionType.Walk, connectedNode2, new LineSegment2D(targetNode.Position, dropTargetSegment.segment.P2), dropTargetSegment.maxHeight);
                    connectedNode2.AddConnection(ConnectionType.Walk, targetNode, new LineSegment2D(dropTargetSegment.segment.P2, targetNode.Position), dropTargetSegment.maxHeight);
                }
            }

            AddConnection(startNode, targetNode, connectionType, maxHeight);
        }

        private static void AddConnection(Node startNode, Node endNode, ConnectionType connectionType, float maxHeight)
        {
            switch (connectionType)
            {
                case ConnectionType.Walk:
                    startNode.AddConnection(ConnectionType.Walk, endNode, new LineSegment2D(startNode.Position, endNode.Position), maxHeight);
                    break;
                case ConnectionType.Drop:
                    startNode.AddConnection(ConnectionType.Drop, endNode, new LineSegment2D(startNode.Position, endNode.Position), maxHeight);
                    break;
                case ConnectionType.Jump:
                    startNode.AddConnection(ConnectionType.Jump, endNode, new LineSegment2D(startNode.Position, endNode.Position), maxHeight);
                    endNode.AddConnection(ConnectionType.Jump, startNode, new LineSegment2D(endNode.Position, startNode.Position), maxHeight);
                    break;
            }
        }
    }
}