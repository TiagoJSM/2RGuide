using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using UnityEditor;
using UnityEngine;

namespace Assets._2RGuide.Editor
{
    public static class EditorNavDrawer
    {
        private static float LineThickness => EditorGUIUtility.pixelsPerPoint * 4;

        public static void RenderWorldNav(NavWorld world)
        {
            var segments = world.Segments;

            RenderSegments(segments, new Color(173f / 255f, 216f / 255f, 230f / 255f), new Color(0, 0, 1.0f));

            if (world.Drops != null)
            {
                RenderDrops(world.Drops, Color.yellow);
            }
            if (world.Jumps != null)
            {
                RenderJumps(world.Jumps, Color.gray);
            }

            RenderNormals(segments, Color.magenta);

            RenderNodes(world.Nodes);
        }

        private static void RenderSegments(NavSegment[] navSegments, Color minHeightColor, Color maxHeightColor)
        {
            if (navSegments == null)
            {
                return;
            }
            var instance = Nav2RGuideSettingsRegister.GetOrCreateSettings();
            foreach (var navSegment in navSegments)
            {
                var segment = navSegment.segment;
                var navTagSetting = instance.GetSettingForNavTag(navSegment.navTag);
                Handles.color = navTagSetting != null
                    ? navTagSetting.Color
                    : Color.Lerp(minHeightColor, maxHeightColor, navSegment.maxHeight / instance.SegmentMaxHeight);
                Handles.DrawLine(segment.P1.ToVector2(), segment.P2.ToVector2(), LineThickness);
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
                RenderArrow(segment.P1.ToVector2(), segment.P2.ToVector2());
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
                RenderArrow(segment.P1.ToVector2(), segment.P2.ToVector2());
                RenderArrow(segment.P2.ToVector2(), segment.P1.ToVector2());
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
            for (var idx = 0; idx < navSegments.Length; idx++)
            {
                var navSegment = navSegments[idx];
                var segment = navSegment.segment;
                var middle = (segment.P2 + segment.P1) / 2;
                RenderArrow(middle.ToVector2(), (middle + segment.NormalizedNormalVector * normalSize).ToVector2(), 0.08f);
#if TWOR_GUIDE_DEBUG
                Handles.Label(middle.ToVector2(), $"Idx: {idx}; N: {segment.NormalizedNormalVector}; Slope: {segment.SlopeDegrees}; MaxH: {navSegment.maxHeight}");
#endif
            }
        }

        private static void RenderArrow(Vector3 pos, Vector3 target, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
        {
            var direction = (target - pos).normalized;

            //arrow shaft
            Handles.DrawLine(pos, target, LineThickness);

            //arrow head
            var right = Quaternion.LookRotation(direction, new Vector3(1.0f, 0.0f, 0.0f)) * Quaternion.Euler(180.0f + arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction, new Vector3(1.0f, 0.0f, 0.0f)) * Quaternion.Euler(180.0f - arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
            Handles.DrawLine(target, target + right * arrowHeadLength, LineThickness);
            Handles.DrawLine(target, target + left * arrowHeadLength, LineThickness);
        }

        private static void RenderNodes(Node[] nodes)
        {
#if TWOR_GUIDE_DEBUG
            if (nodes == null)
            {
                return;
            }
            foreach (var node in nodes)
            {
                Handles.Label(node.Position.ToVector2(), $"Node[{node.NodeIndex}] ({node.Position.x:0.00} , {node.Position.y:0.00})");
            }
#endif
        }
    }
}