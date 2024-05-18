using Assets._2RGuide.Runtime.Helpers;
using NUnit.Framework;
using System;

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