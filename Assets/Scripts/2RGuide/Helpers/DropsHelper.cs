using Assets.Scripts._2RGuide.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class DropsHelper
    {
        public struct Settings
        {
            public float maxDropHeight;
            public float horizontalDistance;
            public float maxSlope;
        }

        public static LineSegment2D[] BuildDrops(List<Node> nodes, LineSegment2D[] segments, Settings settings)
        {
            var resultSegments = new List<LineSegment2D>();

            foreach (var node in nodes.ToArray())
            {
                var canJumpOrDropToLeftSide = node.CanJumpOrDropToLeftSide(settings.maxSlope);
                if (canJumpOrDropToLeftSide)
                {
                    var originX = node.Position.x - settings.horizontalDistance;
                    var target = FindTargetDropSegment(node, segments, originX, settings);
                    if (target)
                    {
                        AddDropTargetNodeForSegment(target, nodes, segments, node);
                        resultSegments.Add(target);
                    }
                }

                var canJumpOrDropToRightSide = node.CanJumpOrDropToRightSide(settings.maxSlope);
                if (canJumpOrDropToRightSide)
                {
                    var originX = node.Position.x + settings.horizontalDistance;
                    var target = FindTargetDropSegment(node, segments, originX, settings);
                    if (target)
                    {
                        AddDropTargetNodeForSegment(target, nodes, segments, node);
                        resultSegments.Add(target);
                    }
                }
            }

            return resultSegments.ToArray();
        }

        private static void AddDropTargetNodeForSegment(LineSegment2D target, List<Node> nodes, LineSegment2D[] segments, Node dropNode)
        {
            var existingNodeAtPosition = nodes.FirstOrDefault(n => n.Position == target.P2);

            if(existingNodeAtPosition != null)
            {
                return;
            }

            var dropTargetSegment = segments.FirstOrDefault(s => s.OnSegment(target.P2));

            if(!dropTargetSegment)
            {
                return;
            }

            var dropTargetNode = new Node() { Position = target.P2 };

            var connectedNode1 = nodes.FirstOrDefault(n => n.Position == dropTargetSegment.P1);
            if(connectedNode1 != null)
            {
                dropTargetNode.Connections.Add(NodeConnection.Walk(connectedNode1, new LineSegment2D(dropTargetSegment.P1, dropTargetNode.Position)));
            }

            var connectedNode2 = nodes.FirstOrDefault(n => n.Position == dropTargetSegment.P2);
            if (connectedNode2 != null)
            {
                dropTargetNode.Connections.Add(NodeConnection.Walk(connectedNode2, new LineSegment2D(dropTargetNode.Position, dropTargetSegment.P2)));
            }

            dropNode.Connections.Add(NodeConnection.Drop(dropTargetNode, new LineSegment2D(dropNode.Position, dropTargetNode.Position)));
            nodes.Add(dropTargetNode);
        }

        //ToDo: Check if doesn't collide with any other collider not part of pathfinding
        private static LineSegment2D FindTargetDropSegment(Node node, LineSegment2D[] segments, float originX, Settings settings)
        {
            var origin = new Vector2(originX, node.Position.y);

            var segment = segments.Where(s =>
            {
                var position = s.PositionInX(originX);
                if(!position.HasValue)
                {
                    return false;
                }
                if(origin.y < position.Value.y)
                {
                    return false;
                }
                return Vector2.Distance(position.Value, origin) <= settings.maxDropHeight;
            })
            .MinBy(s =>
            {
                var position = s.PositionInX(originX);
                return Vector2.Distance(position.Value, origin);
            });
            if (segment)
            {
                return new LineSegment2D(node.Position, segment.PositionInX(originX).Value);
            }
            return default;
        }
    }
}