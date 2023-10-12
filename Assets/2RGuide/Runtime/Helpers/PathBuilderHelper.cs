using _2RGuide.Math;
using System.Collections.Generic;
using System.Linq;

namespace _2RGuide.Helpers
{
    public static class PathBuilderHelper
    {
        public static void AddTargetNodeForSegment(LineSegment2D target, NodeStore nodeStore, List<NavSegment> navSegments, Node startNode, ConnectionType connectionType, float maxSlope, float maxHeight)
        {
            var dropTargetSegment = navSegments.FirstOrDefault(ss => !ss.segment.OverMaxSlope(maxSlope) && ss.segment.OnSegment(target.P2));

            if (!dropTargetSegment)
            {
                return;
            }

            var targetNode = nodeStore.SplitSegmentAt(dropTargetSegment.segment, target.P2);
            navSegments.Remove(dropTargetSegment);

            navSegments.Add(new NavSegment()
            {
                segment = new LineSegment2D(dropTargetSegment.segment.P1, targetNode.Position),
                maxHeight = dropTargetSegment.maxHeight,
                oneWayPlatform = dropTargetSegment.oneWayPlatform
            });
            navSegments.Add(new NavSegment()
            {
                segment = new LineSegment2D(targetNode.Position, dropTargetSegment.segment.P2),
                maxHeight = dropTargetSegment.maxHeight,
                oneWayPlatform = dropTargetSegment.oneWayPlatform
            });

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