using Assets._2RGuide.Runtime.Math;
using Assets._2RGuide.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tests.PlayModeTests
{
    public class PolyTreeTests
    {
        [Test]
        public void IsPointInside()
        {
            var polygons = new Polygon[]
            {
                new Polygon(new [] { new RGuideVector2(-100f, -100f), new RGuideVector2(-100f, 100f), new RGuideVector2(100f, 100f), new RGuideVector2(100f, -100f) }),
                new Polygon(new [] { new RGuideVector2(-10f, -10f), new RGuideVector2(-10f, 10f), new RGuideVector2(10f, 10f), new RGuideVector2(10f, -10f) }),
                new Polygon(new [] { new RGuideVector2(-1f, -1f), new RGuideVector2(-1f, 1f), new RGuideVector2(1f, 1f), new RGuideVector2(1f, -1f) }),
            };
            var polyTree = new PolyTree(polygons);

            Assert.IsTrue(polyTree.IsPointInside(RGuideVector2.zero));
        }
    }
}
