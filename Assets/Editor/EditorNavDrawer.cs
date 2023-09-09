using Assets.Scripts._2RGuide;
using Assets.Scripts._2RGuide.Math;
using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Editor
{
    public static class EditorNavDrawer
    {
        public static void RenderWorldNav(NavWorld world)
        {
            var segments = world.segments;

            RenderSegments(segments, Color.blue);

            if (world.drops != null)
            {
                RenderDrops(world.drops, Color.yellow);
            }
            if (world.jumps != null)
            {
                RenderJumps(world.jumps, Color.gray);
            }

            RenderNormals(world.segments, Color.magenta);

            RenderNodes(world.nodes);
        }

        private static void RenderSegments(LineSegment2D[] segments, Color lineColor)
        {
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                Handles.DrawLine(segment.P1, segment.P2, EditorGUIUtility.pixelsPerPoint * 3);
            }
        }

        private static void RenderDrops(LineSegment2D[] segments, Color lineColor)
        {
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                RenderArrow(segment.P1, segment.P2);
            }
        }

        private static void RenderJumps(LineSegment2D[] segments, Color lineColor)
        {
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                RenderArrow(segment.P1, segment.P2);
                RenderArrow(segment.P2, segment.P1);
            }
        }

        private static void RenderNormals(LineSegment2D[] segments, Color lineColor)
        {
            const float normalSize = 0.2f;
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                var middle = (segment.P2 + segment.P1) / 2;
                RenderArrow(middle, middle + segment.NormalVector.normalized * normalSize, 0.08f);
                Handles.Label(middle, $"N: {segment.NormalVector.normalized}; Slope: {segment.Slope}");
            }
        }

        private static void RenderArrow(Vector3 pos, Vector3 target, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
        {
            var direction = (target - pos).normalized;

            //arrow shaft
            Handles.DrawLine(pos, target, EditorGUIUtility.pixelsPerPoint * 3);

            //arrow head
            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(180.0f + arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(180.0f - arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
            Handles.DrawLine(target, target + right * arrowHeadLength, EditorGUIUtility.pixelsPerPoint * 3);
            Handles.DrawLine(target, target + left * arrowHeadLength, EditorGUIUtility.pixelsPerPoint * 3);
        }

        private static void RenderNodes(Node[] nodes)
        {
            foreach (var node in nodes)
            {
                Handles.Label(node.Position, $"Node ({node.Position.x:0.00} , {node.Position.y:0.00})");
            }
        }
    }
}