using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class Vector2Extensions
    {
        public static bool Approximately(this Vector2 v, Vector2 other)
        {
            return Mathf.Approximately(v.x, other.x) && Mathf.Approximately(v.y, other.y);
        }
    }
}