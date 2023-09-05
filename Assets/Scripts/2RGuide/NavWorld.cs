using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide
{
    //investigate GeometryUtils from https://docs.unity3d.com/Packages/com.unity.reflect@1.0/api/Unity.Labs.Utils.GeometryUtils.html
    public class NavWorld : MonoBehaviour
    {
        public Node[] nodes;
        public LineSegment2D[] segments;
        public LineSegment2D[] drops;
        public LineSegment2D[] jumps;
    }
}