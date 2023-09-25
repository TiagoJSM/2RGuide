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
            Assert.AreEqual(new Vector3(-5.0f, 0.5f, 0.0f), agent.transform.position);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(new Vector3(5.0f, 0.5f, 0.0f), agent.transform.position);
        }

        [UnityTest]
        public IEnumerator VerifyAgentReachedGoalWithJumps()
        {
            SceneManager.LoadScene("MoveToPositionWithJumpsTestScene");
            yield return null;
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            var comparer = new Vector3EqualityComparer(0.25f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(1.56f, -3.45f, 0.0f)).Using(comparer));
            yield return new WaitForSeconds(1.0f);
            Assert.That(agent.transform.position, Is.EqualTo(new Vector3(6.62f, 6.34f, 0.0f)).Using(comparer));
        }
    }
}