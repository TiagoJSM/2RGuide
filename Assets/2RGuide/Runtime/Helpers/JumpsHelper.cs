using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class JumpsHelper
    {
        public static LineSegment2D[] BuildJumps(NavBuildContext navBuildContext, NodeStore nodes, AirConnectionHelper.Settings settings)
        {
            return AirConnectionHelper.Build(navBuildContext, nodes, new LineSegment2D[0], ConnectionType.Jump, ConnectionType.OneWayPlatformJump, settings);
        }
    }
}