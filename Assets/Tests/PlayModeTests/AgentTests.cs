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
        [SetUp]
        public void Setup()
        {
            SceneManager.LoadScene("MoveToPositionTestScene");
        }

        [UnityTest]
        public IEnumerator VerifyAgentReachedGoal()
        {
            var agent = GameObject.Find("Agent");
            Assert.That(agent, Is.Not.Null);
            Assert.AreEqual(agent.transform.position, new Vector3(-5.0f, 0.5f, 0.0f));
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(agent.transform.position, new Vector3(5.0f, 0.5f, 0.0f));
        }

        [TearDown]
        public void Teardown()
        {
            //EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }
}