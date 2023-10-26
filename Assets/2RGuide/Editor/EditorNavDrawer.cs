﻿using _2RGuide;
using _2RGuide.Helpers;
using _2RGuide.Math;
using UnityEditor;
using UnityEngine;

namespace _2RGuide.Editor
{
    public static class EditorNavDrawer
    {
        private static float LineThickness => EditorGUIUtility.pixelsPerPoint * 4;

        public static void RenderWorldNav(NavWorld world)
        {
            var segments = world.segments;

            RenderSegments(segments, new Color(173f / 255f, 216f / 255f, 230f / 255f), new Color(0, 0, 1.0f));

            if (world.drops != null)
            {
                RenderDrops(world.drops, Color.yellow);
            }
            if (world.jumps != null)
            {
                RenderJumps(world.jumps, Color.gray);
            }

            RenderNormals(segments, Color.magenta);

            RenderNodes(world.nodeStore.ToArray());
        }

        private static void RenderSegments(NavSegment[] navSegments, Color minHeightColor, Color maxHeightColor)
        {
            if(navSegments == null)
            {
                return;
            }
            var instance = Nav2RGuideSettings.GetOrCreateSettings();
            foreach (var navSegment in navSegments)
            {
                var segment = navSegment.segment;
                Handles.color = Color.Lerp(minHeightColor, maxHeightColor, navSegment.maxHeight / instance.SegmentMaxHeight);
                Handles.DrawLine(segment.P1, segment.P2, LineThickness);
            }
        }

        private static void RenderDrops(LineSegment2D[] segments, Color lineColor)
        {
            if (segments == null)
            {
                return;
            }
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                RenderArrow(segment.P1, segment.P2);
            }
        }

        private static void RenderJumps(LineSegment2D[] segments, Color lineColor)
        {
            if (segments == null)
            {
                return;
            }
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                RenderArrow(segment.P1, segment.P2);
                RenderArrow(segment.P2, segment.P1);
            }
        }

        private static void RenderNormals(NavSegment[] navSegments, Color lineColor)
        {
            if (navSegments == null)
            {
                return;
            }
            const float normalSize = 0.2f;
            Handles.color = lineColor;
            foreach (var navSegment in navSegments)
            {
                var segment = navSegment.segment;
                var middle = (segment.P2 + segment.P1) / 2;
                RenderArrow(middle, middle + segment.NormalVector.normalized * normalSize, 0.08f);
                Handles.Label(middle, $"N: {segment.NormalVector.normalized}; Slope: {segment.Slope}");
            }
        }

        private static void RenderArrow(Vector3 pos, Vector3 target, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
        {
            var direction = (target - pos).normalized;

            //arrow shaft
            Handles.DrawLine(pos, target, LineThickness);

            //arrow head
            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(180.0f + arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(180.0f - arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
            Handles.DrawLine(target, target + right * arrowHeadLength, LineThickness);
            Handles.DrawLine(target, target + left * arrowHeadLength, LineThickness);
        }

        private static void RenderNodes(Node[] nodes)
        {
            if(nodes == null)
            {
                return;
            }
            foreach (var node in nodes)
            {
                Handles.Label(node.Position, $"Node[{node.NodeIndex}] ({node.Position.x:0.00} , {node.Position.y:0.00})");
            }
        }
    }
}