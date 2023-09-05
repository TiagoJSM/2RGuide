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

        public static LineSegment2D[] BuildDrops(List<Node> nodes, LineSegment2D[] segments, LineSegment2D[] jumps, Settings settings)
        {
            var resultSegments = new List<LineSegment2D>();

            foreach (var node in nodes.ToArray())
            {
                var canJumpOrDropToLeftSide = node.CanJumpOrDropToLeftSide(settings.maxSlope);
                if (canJumpOrDropToLeftSide)
                {
                    var originX = node.Position.x - settings.horizontalDistance;
                    var target = FindTargetDropSegment(node, segments, jumps, originX, settings);
                    if (target)
                    {
                        PathBuilderHelper.AddTargetNodeForSegment(target, nodes, segments, node, ConnectionType.Drop);
                        resultSegments.Add(target);
                    }
                }

                var canJumpOrDropToRightSide = node.CanJumpOrDropToRightSide(settings.maxSlope);
                if (canJumpOrDropToRightSide)
                {
                    var originX = node.Position.x + settings.horizontalDistance;
                    var target = FindTargetDropSegment(node, segments, jumps, originX, settings);
                    if (target)
                    {
                        PathBuilderHelper.AddTargetNodeForSegment(target, nodes, segments, node, ConnectionType.Drop);
                        resultSegments.Add(target);
                    }
                }
            }

            return resultSegments.ToArray();
        }

        //ToDo: Check if doesn't collide with any other collider not part of pathfinding
        // Check if there's no jump segment as replacement
        private static LineSegment2D FindTargetDropSegment(Node node, LineSegment2D[] segments, LineSegment2D[] jumps, float originX, Settings settings)
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
                var dropSegment = new LineSegment2D(node.Position, segment.PositionInX(originX).Value);

                if(!jumps.Any(rs => rs.IsCoincident(dropSegment)))
                {
                    return dropSegment;
                }
            }
            return default;
        }
    }
}