using Assets._2RGuide.Runtime;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    public class SearchTests : MonoBehaviour
    {
        [UnityTest]
        public IEnumerator FindNavSegment()
        {
            SceneManager.LoadScene("NavSegmentFinderTest");
            yield return null;
            var segment = NavWorldReference.Instance.NavWorld.Search(new RTree.Envelope(0, 0, 10, 10));
            Assert.AreEqual(4, segment.Count());
        }
    }
}