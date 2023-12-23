using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class Vector2Extensions
    {
        public static bool Approximately(this Vector2 v, Vector2 other)
        {
            return v.x.Approximately(other.x) && v.y.Approximately(other.y);
        }
    }
}