using NUnit.Framework;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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
            Assert.AreEqual(agent.transform.position, new Vector3(-5.0f, 0.5f, 0.0f));
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(agent.transform.position, new Vector3(5.0f, 0.5f, 0.0f));
        }

        [UnityTest]
        public IEnumerator VerifyAgentReachedGoalWithJumps()
        {
            SceneManager.LoadScene("MoveToPositionWithJumpsTestScene");
            yield return null;
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            Assert.AreEqual(agent.transform.position, new Vector3(1.565005f, -3.45f, 0.0f));
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(agent.transform.position, new Vector3(6.62f, 6.38f, 0.0f));
        }
    }
}