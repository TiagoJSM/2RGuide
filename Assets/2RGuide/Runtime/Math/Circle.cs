using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    public struct Circle
    {
        public RGuideVector2 center;
        public float radius;

        public Circle(RGuideVector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public bool IsInside(RGuideVector2 point)
        {
            return RGuideVector2.Distance(center, point) < radius;
        }
    }
}