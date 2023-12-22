using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class DropsHelper
    {
        public static LineSegment2D[] BuildDrops(NavBuildContext navBuildContext, NodeStore nodes, LineSegment2D[] jumps, AirConnectionHelper.Settings settings)
        {
            return AirConnectionHelper.Build(navBuildContext, nodes, jumps, ConnectionType.Drop, ConnectionType.OneWayPlatformDrop, settings);
        }
    }
}