using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Tests.PlayModeTests
{
    public class IEnumerableExtensionsTests
    {
        [Test]
        public void TestMinByOnTuples()
        {
            var values = new Tuple<int>[]
            {
                new Tuple<int>(10),
                new Tuple<int>(0),
                new Tuple<int>(20),
            };

            var min = values.MinBy(x => x.Item1);

            Assert.AreEqual(0, min.Item1);
        }

        [Test]
        public void TestMinByOnEmptyArray()
        {
            var values = new Tuple<int>[] { };

            var min = values.MinBy(x => x.Item1);

            Assert.AreEqual(null, min);
        }
    }
}