using Assets._2RGuide.Runtime.Helpers;
using Clipper2Lib;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets._2RGuide.Runtime.Math
{
    // Taken from https://discussions.unity.com/t/world-coordinates-of-boxcollider2d/125846/2
    public struct Box
    {
        public Vector2 TopLeft { get; private set; }
        public Vector2 TopRight { get; private set; }
        public Vector2 BottomLeft { get; private set; }
        public Vector2 BottomRight { get; private set; }
        

        public Box(BoxCollider2D collider)
        {
            TopLeft = Vector2.zero;
            TopRight = Vector2.zero;
            BottomLeft = Vector2.zero;
            BottomRight = Vector2.zero;

            var bcTransform = collider.transform;

            // The collider's centre point in the world
            Vector2 worldPosition = bcTransform.TransformPoint(collider.offset);

            // The collider's local width and height, accounting for scale, divided by 2
            var size = new Vector2(collider.size.x * bcTransform.localScale.x * 0.5f, collider.size.y * bcTransform.localScale.y * 0.5f);

            // STEP 1: FIND LOCAL, UN-ROTATED CORNERS
            // Find the 4 corners of the BoxCollider2D in LOCAL space, if the BoxCollider2D had never been rotated
            var corner1 = new Vector2(-size.x, -size.y);
            var corner2 = new Vector2(-size.x, size.y);
            var corner3 = new Vector2(size.x, -size.y);
            var corner4 = new Vector2(size.x, size.y);

            Initialize(ref this, bcTransform, worldPosition, corner1, corner2, corner3, corner4);
        }

        public Box(SpriteRenderer sprite)
        {
            TopLeft = Vector2.zero;
            TopRight = Vector2.zero;
            BottomLeft = Vector2.zero;
            BottomRight = Vector2.zero;

            var bcTransform = sprite.transform;

            // The collider's centre point in the world
            Vector2 worldPosition = bcTransform.position;

            // The collider's local width and height, accounting for scale, divided by 2
            var size = new Vector2(sprite.size.x * bcTransform.localScale.x * 0.5f, sprite.size.y * bcTransform.localScale.y * 0.5f);

            // STEP 1: FIND LOCAL, UN-ROTATED CORNERS
            // Find the 4 corners of the BoxCollider2D in LOCAL space, if the BoxCollider2D had never been rotated
            var corner1 = new Vector2(-size.x, -size.y);
            var corner2 = new Vector2(-size.x, size.y);
            var corner3 = new Vector2(size.x, -size.y);
            var corner4 = new Vector2(size.x, size.y);

            Initialize(ref this, bcTransform, worldPosition, corner1, corner2, corner3, corner4);
        }

        private static void Initialize(ref Box box, Transform transform, Vector2 worldPosition, Vector2 unrotatedBottomLeft, Vector2 unrotatedTopLeft, Vector2 unrotatedBottomRight, Vector2 unrotatedTopRight)
        {
            // STEP 2: ROTATE CORNERS
            // Rotate those 4 corners around the centre of the collider to match its transform.rotation
            var eulerAngles = transform.eulerAngles;
            unrotatedBottomLeft = RotatePointAroundPivot(unrotatedBottomLeft, Vector3.zero, eulerAngles);
            unrotatedTopLeft = RotatePointAroundPivot(unrotatedTopLeft, Vector3.zero, eulerAngles);
            unrotatedBottomRight = RotatePointAroundPivot(unrotatedBottomRight, Vector3.zero, eulerAngles);
            unrotatedTopRight = RotatePointAroundPivot(unrotatedTopRight, Vector3.zero, eulerAngles);

            // STEP 3: FIND WORLD POSITION OF CORNERS
            // Add the 4 rotated corners above to our centre position in WORLD space - and we're done!
            box.BottomLeft = worldPosition + unrotatedBottomLeft;
            box.TopLeft = worldPosition + unrotatedTopLeft;
            box.BottomRight = worldPosition + unrotatedBottomRight;
            box.TopRight = worldPosition + unrotatedTopRight;
        }

        // Helper method courtesy of @aldonaletto
        // http://answers.unity3d.com/questions/532297/rotate-a-vector-around-a-certain-point.html
        private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }
    }
}