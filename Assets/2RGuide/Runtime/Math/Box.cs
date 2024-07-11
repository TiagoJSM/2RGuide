using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets._2RGuide.Runtime.Math
{
    // Taken from https://discussions.unity.com/t/world-coordinates-of-boxcollider2d/125846/2
    public struct Box
    {
        public Vector2 TopLeft { get; }
        public Vector2 TopRight { get; }
        public Vector2 BottomLeft { get; }
        public Vector2 BottomRight { get; }

        public Box(BoxCollider2D collider)
        {
            var bcTransform = collider.transform;

            // The collider's centre point in the world
            Vector2 worldPosition = bcTransform.TransformPoint(0, 0, 0);

            // The collider's local width and height, accounting for scale, divided by 2
            var size = new Vector2(collider.size.x * bcTransform.localScale.x * 0.5f, collider.size.y * bcTransform.localScale.y * 0.5f);

            // STEP 1: FIND LOCAL, UN-ROTATED CORNERS
            // Find the 4 corners of the BoxCollider2D in LOCAL space, if the BoxCollider2D had never been rotated
            var corner1 = new Vector2(-size.x, -size.y);
            var corner2 = new Vector2(-size.x, size.y);
            var corner3 = new Vector2(size.x, -size.y);
            var corner4 = new Vector2(size.x, size.y);

            // STEP 2: ROTATE CORNERS
            // Rotate those 4 corners around the centre of the collider to match its transform.rotation
            corner1 = RotatePointAroundPivot(corner1, Vector3.zero, bcTransform.eulerAngles);
            corner2 = RotatePointAroundPivot(corner2, Vector3.zero, bcTransform.eulerAngles);
            corner3 = RotatePointAroundPivot(corner3, Vector3.zero, bcTransform.eulerAngles);
            corner4 = RotatePointAroundPivot(corner4, Vector3.zero, bcTransform.eulerAngles);

            // STEP 3: FIND WORLD POSITION OF CORNERS
            // Add the 4 rotated corners above to our centre position in WORLD space - and we're done!
            BottomLeft = worldPosition + corner1;
            TopLeft = worldPosition + corner2;
            BottomRight = worldPosition + corner3;
            TopRight = worldPosition + corner4;

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