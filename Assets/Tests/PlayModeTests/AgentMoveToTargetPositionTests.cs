using Assets._2RGuide.Runtime;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using static Assets._2RGuide.Runtime.GuideAgent;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools.Utils;
using UnityEngine.TestTools;
using System.Linq;
using Assets._2RGuide.Runtime.Math;

namespace Assets.Tests.PlayModeTests
{
    public class AgentMoveToTargetPositionTests
    {
        [UnityTest]
        public IEnumerator MoveToTargetsTest()
        {
            Debug.Log($"Test running on scene MoveToTargetTestScene");
            SceneManager.LoadScene("MoveToTargetTestScene");
            yield return null;

            var agentGO = GameObject.Find("Agent");
            Assert.That(agentGO, Is.Not.Null);
            var agent = agentGO.GetComponent<TransformMovement>();
            Assert.That(agent, Is.Not.Null);

            var target = GameObject.Find("Target");
            var positions = new [] { "Position1", "Position2" }.Select(t => GameObject.Find(t)).ToArray();

            Assert.That(target, Is.Not.Null);
            foreach (var position in positions)
            {
                Assert.That(position, Is.Not.Null);
            }

            var comparer = new Vector3EqualityComparer(0.25f);

            var positionIdx = 0;

            while(positionIdx < positions.Count())
            {
                target.transform.position = positions[positionIdx].transform.position;
                yield return new WaitForSeconds(0.05f);
                if (RGuideVector2.Distance(new RGuideVector2(agentGO.transform.position), new RGuideVector2(target.transform.position)) < 0.5f)
                {
                    positionIdx++;
                }
            }

            yield return new WaitForSeconds(5.0f);
            Assert.That(agent.GuideAgent.CurrentPathStatus == PathStatus.Complete);
            Assert.That(agent.transform.position, Is.EqualTo(target.transform.position).Using(comparer));
        }
    }
}