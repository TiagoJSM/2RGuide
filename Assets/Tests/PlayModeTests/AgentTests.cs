using _2RGuide;
using NUnit.Framework;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Assets.Tests.PlayModeTests
{
    public class AgentTests
    {

        [UnityTest]
        public IEnumerator VerifyAgentReachedGoalWalking()
        {
            SceneManager.LoadScene("MoveToPositionTestScene");
            yield return null;
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            Assert.AreEqual(new Vector3(-5.0f, 0.5f), agent.transform.position);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(new Vector3(5.0f, 0.5f), agent.transform.position);
        }

        [UnityTest]
        public IEnumerator VerifyAgentReachedGoalWithJumps()
        {
            SceneManager.LoadScene("MoveToPositionWithJumpsTestScene");
            yield return null;
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            var comparer = new Vector3EqualityComparer(0.25f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(1.56f, -3.45f)).Using(comparer));
            yield return new WaitForSeconds(1.0f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(6.62f, 6.34f)).Using(comparer));
        }


        [UnityTest]
        public IEnumerator VerifyAgentReachedGoalWithOneWayPlatformJumps()
        {
            SceneManager.LoadScene("MoveToPositionOneWayJumpTestScene");
            yield return null;
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            var comparer = new Vector3EqualityComparer(0.25f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(0.93f, 0.5f)).Using(comparer));
            yield return new WaitForSeconds(1.0f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(0.934f, 3.06f)).Using(comparer));
        }

        [UnityTest]
        public IEnumerator VerifyAgentReachedGoalInAdjacentSegment()
        {
            SceneManager.LoadScene("MoveToAdjacentSegmentTestScene");
            yield return null;
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            var comparer = new Vector3EqualityComparer(0.25f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(2.94f, -3.45f)).Using(comparer));
            yield return new WaitForSeconds(1.0f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(-1.298f, -3.598f)).Using(comparer));
        }
    }
}