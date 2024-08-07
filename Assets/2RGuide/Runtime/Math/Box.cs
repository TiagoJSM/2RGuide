﻿using Assets._2RGuide.Runtime.Helpers;
using Clipper2Lib;
using System.Collections;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets._2RGuide.Runtime.Math
{
    public struct Box
    {
        public Vector2 TopLeft { get; private set; }
        public Vector2 TopRight { get; private set; }
        public Vector2 BottomLeft { get; private set; }
        public Vector2 BottomRight { get; private set; }
        
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

            TopRight = collider.transform.TransformPoint(new Vector3(maxX, maxY));
            TopLeft = collider.transform.TransformPoint(new Vector3(minX, maxY));
            BottomRight = collider.transform.TransformPoint(new Vector3(maxX, minY));
            BottomLeft = collider.transform.TransformPoint(new Vector3(minX, minY));
        }
    }
}