using Assets.Scripts;
using Assets.Scripts._2RGuide;
using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using Clipper2Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace Assets.Editor
{
    [CustomEditor(typeof(NavWorld))]
    public class WorldEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var world = (NavWorld)target;
            if (GUILayout.Button("Bake Pathfinding"))
            {
                CollectSegments(world);

                var nodes = NodeHelpers.BuildNodes(world.segments);
                var drops = DropsHelper.BuildDrops(
                    nodes,
                    world.segments,
                    new DropsHelper.Settings()
                    {
                        //Nav2RGuideSettings.instance...
                        maxDropHeight = world.maxDropHeight,
                        horizontalDistance = world.horizontalDistance,
                        maxSlope = world.maxSlope
                    });
                var jumps = JumpsHelper.BuildJumps(
                    nodes,
                    world.segments,
                    new JumpsHelper.Settings()
                    {
                        //Nav2RGuideSettings.instance...
                        maxJumpDistance = world.maxJumpDistance,
                        maxSlope = world.maxSlope
                    });

                world.nodes = nodes.ToArray();
                world.drops = drops;
                world.jumps = jumps;

                EditorUtility.SetDirty(world);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private PathsD UnionShapes(PathsD shapes)
        {
            var fillrule = FillRule.NonZero;
            return Clipper.Union(shapes, fillrule);
        }

        private void CollectSegments(NavWorld world)
        {
            var paths = new PathsD();
            var children = world.transform.childCount;
            for (var idx = 0; idx < children; idx++)
            {
                var child = world.transform.GetChild(idx);
                if (child.gameObject.activeInHierarchy)
                {
                    var collider = child.GetComponent<Collider2D>();
                    CollectSegments(collider, paths);
                }
            }
            paths = UnionShapes(paths);
            var segments = ConvertToSegments(paths);
            AssignSegments(segments);
        }

        private LineSegment2D[] ConvertToSegments(PathsD paths)
        {
            var segments = new List<LineSegment2D>();

            foreach (var path in paths)
            {
                // Clippy paths are created in the reverse order
                path.Reverse();

                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)p2.x, (float)p2.y)));
                    p1 = p2;
                }
                var start = path[0];
                segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)start.x, (float)start.y)));
            }

            return segments.ToArray();
        }

        private void CollectSegments(Collider2D collider, PathsD paths)
        {
            if(collider is BoxCollider2D box)
            {
                CollectSegments(box, paths);
            }
            //else if(collider is EdgeCollider2D edge)
            //{
            //    CollectSegments(edge, paths);
            //}
            else if (collider is PolygonCollider2D polygon)
            {
                CollectSegments(polygon, paths);
            }
            else if (collider is CompositeCollider2D composite)
            {
                CollectSegments(composite, paths);
            }
        }

        private void CollectSegments(BoxCollider2D collider, PathsD paths)
        {
            var bounds = collider.bounds;

            var shape = Clipper.MakePath(new double[] 
                { 
                    bounds.min.x, bounds.min.y, 
                    bounds.min.x, bounds.max.y,
                    bounds.max.x, bounds.max.y,
                    bounds.max.x, bounds.min.y,
                });

            paths.Add(shape);
        }

        //private void CollectSegments(EdgeCollider2D collider, PathsD paths)
        //{
        //    if(collider.pointCount < 1)
        //    {
        //        return;
        //    }

        //    var p1 = collider.transform.TransformPoint(collider.points[0]);
        //    for (var idx = 1; idx < collider.pointCount; idx++)
        //    {
        //        var p2 = collider.transform.TransformPoint(collider.points[idx]);
        //        buffer.Add(new Segment { p1 = p1, p2 = p2 });
        //        p1 = p2;
        //    }
        //}

        private void CollectSegments(PolygonCollider2D collider, PathsD paths)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = collider.GetPath(pathIdx);

                if (path.Length < 1)
                {
                    return;
                }

                var points = path.SelectMany(p => 
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                var shape = Clipper.MakePath(points.ToArray());

                paths.Add(shape);
            }
        }

        private void CollectSegments(CompositeCollider2D collider, PathsD paths)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = new List<Vector2>();
                var _ = collider.GetPath(pathIdx, path);

                if (path.Count < 1)
                {
                    return;
                }

                var points = path.SelectMany(p =>
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                var shape = Clipper.MakePath(points.ToArray());

                paths.Add(shape);
            }
        }

        private void AssignSegments(LineSegment2D[] segments)
        {
            var world = (NavWorld)target;
            world.segments = segments;
        }

        private void OnSceneGUI()
        {
            var world = (NavWorld)target;
            var segments = world.segments;

            RenderSegments(segments, Color.blue);

            if(world.drops != null)
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

        private void RenderSegments(LineSegment2D[] segments, Color lineColor)
        {
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                Handles.DrawLine(segment.P1, segment.P2, EditorGUIUtility.pixelsPerPoint * 3);
            }
        }

        private void RenderDrops(LineSegment2D[] segments, Color lineColor)
        {
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                RenderArrow(segment.P1, segment.P2);
            }
        }

        private void RenderJumps(LineSegment2D[] segments, Color lineColor)
        {
            Handles.color = lineColor;
            foreach (var segment in segments)
            {
                RenderArrow(segment.P1, segment.P2);
                RenderArrow(segment.P2, segment.P1);
            }
        }

        private void RenderNormals(LineSegment2D[] segments, Color lineColor)
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