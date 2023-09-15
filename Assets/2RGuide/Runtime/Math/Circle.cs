using UnityEngine;

namespace _2RGuide.Math
{
    public struct Circle
    {
        public Vector2 center;
        public float radius;

        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public bool IsInside(Vector2 point)
        {
            return Vector2.Distance(center, point) < radius;
        }
    }
}