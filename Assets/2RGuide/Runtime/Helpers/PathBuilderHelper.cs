using _2RGuide.Math;
using System.Linq;

namespace _2RGuide.Helpers
{
    public static class PathBuilderHelper
    {
        public static void AddTargetNodeForSegment(LineSegment2D target, NodeStore nodeStore, NavSegment[] navSegments, Node startNode, ConnectionType connectionType, float maxSlope, float maxHeight)
        {
            var dropTargetSegment = navSegments.FirstOrDefault(ss => !ss.segment.OverMaxSlope(maxSlope) && ss.segment.OnSegment(target.P2));

            if (!dropTargetSegment)
            {
                return;
            }

            var targetNode = nodeStore.SplitSegmentAt(dropTargetSegment.segment, target.P2);

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