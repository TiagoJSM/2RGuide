using _2RGuide.Helpers;
using _2RGuide.Math;
using UnityEngine;

namespace _2RGuide
{
    //investigate GeometryUtils from https://docs.unity3d.com/Packages/com.unity.reflect@1.0/api/Unity.Labs.Utils.GeometryUtils.html
    public class NavWorld : MonoBehaviour
    {
        public NodeStore nodeStore;
        public NavSegment[] segments;
        public LineSegment2D[] drops;
        public LineSegment2D[] jumps;
    }
}