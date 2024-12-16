using Assets._2RGuide.Runtime.Helpers;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    public struct Box
    {
        public RGuideVector2 TopLeft { get; private set; }
        public RGuideVector2 TopRight { get; private set; }
        public RGuideVector2 BottomLeft { get; private set; }
        public RGuideVector2 BottomRight { get; private set; }
        
        public Box(BoxCollider2D collider)
        {
            var mesh = collider.CreateMesh(true, true);
            var vertices = mesh.vertices;
            var eulerAngles = collider.transform.eulerAngles;
            var scale = collider.transform.localScale;
            var transformMatrix = collider.transform.localToWorldMatrix;
            var inverseTransform = transformMatrix.inverse;

            var unrotatedVertices = vertices.Select(collider.transform.InverseTransformPoint).ToArray();
            var minX = unrotatedVertices.MinBy(v => v.x).x;
            var maxX = unrotatedVertices.MaxBy(v => v.x).x;
            var minY = unrotatedVertices.MinBy(v => v.y).y;
            var maxY = unrotatedVertices.MaxBy(v => v.y).y;

            TopRight = new RGuideVector2(collider.transform.TransformPoint(new Vector3(maxX, maxY)));
            TopLeft = new RGuideVector2(collider.transform.TransformPoint(new Vector3(minX, maxY)));
            BottomRight = new RGuideVector2(collider.transform.TransformPoint(new Vector3(maxX, minY)));
            BottomLeft = new RGuideVector2(collider.transform.TransformPoint(new Vector3(minX, minY)));
        }

        public Box(RGuideVector2 center, RGuideVector2 size)
        {
            var extents = size * 0.5f;
            TopLeft = center + new RGuideVector2(-extents.x, extents.y);
            BottomLeft = center + new RGuideVector2(-extents.x, -extents.y);
            TopRight = center + new RGuideVector2(extents.x, extents.y);
            BottomRight = center + new RGuideVector2(extents.x, -extents.y);
        }
    }
}