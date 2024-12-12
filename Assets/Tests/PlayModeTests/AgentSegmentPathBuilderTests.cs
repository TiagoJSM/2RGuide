using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using static Assets._2RGuide.Runtime.AgentOperations;

namespace Assets.Tests.PlayModeTests
{
    public class AgentSegmentPathBuilderTests
    {
        public class BuildPathFromParams
        {
            public Node[] Path { get; }
            public RGuideVector2 Start { get; }
            public RGuideVector2 Target { get; }
            public AgentSegment[] Result { get; }

            public BuildPathFromParams(Node[] path, RGuideVector2 start, RGuideVector2 target, AgentSegment[] result)
            {
                Path = path;
                Start = start;
                Target = target;
                Result = result;
            }
        }

        static readonly BuildPathFromParams[] BuildPathFromTestValues = new[]
        {
            new BuildPathFromParams(
                BuildSequentialNodePath(new RGuideVector2(0f, 0f), new RGuideVector2(7f, 0f)),
                new RGuideVector2(2f, 0f),
                new RGuideVector2(5f, 0f),
                new[]
                {
                    new AgentSegment(new RGuideVector2(2f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(5f, 0f), ConnectionType.Walk)
                }),
            new BuildPathFromParams(
                BuildSequentialNodePath(new RGuideVector2(0f, 0f), new RGuideVector2(7f, 0f)),
                new RGuideVector2(5f, 0f),
                new RGuideVector2(2f, 0f),
                new[]
                {
                    new AgentSegment(new RGuideVector2(5f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(2f, 0f), ConnectionType.Walk)
                }),
            new BuildPathFromParams(
                BuildSequentialNodePath(new RGuideVector2(0f, 0f), new RGuideVector2(5f, 0f), new RGuideVector2(10f, 0f)),
                new RGuideVector2(2f, 0f),
                new RGuideVector2(7f, 0f),
                new[]
                {
                    new AgentSegment(new RGuideVector2(2f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(5f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(7f, 0f), ConnectionType.Walk)
                }),
            new BuildPathFromParams(
                BuildSequentialNodePath(new RGuideVector2(10f, 0f), new RGuideVector2(5f, 0f), new RGuideVector2(0f, 0f)),
                new RGuideVector2(7f, 0f),
                new RGuideVector2(2f, 0f),
                new[]
                {
                    new AgentSegment(new RGuideVector2(7f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(5f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(2f, 0f), ConnectionType.Walk)
                }),
            new BuildPathFromParams(
                BuildSequentialNodePath(new RGuideVector2(0f, 0f), new RGuideVector2(5f, 0f), new RGuideVector2(5f, 0.5f), new RGuideVector2(10f, 0.5f)),
                new RGuideVector2(2f, 0f),
                new RGuideVector2(7f, 0f),
                new[]
                {
                    new AgentSegment(new RGuideVector2(2f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(5f, 0f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(5f, 0.5f), ConnectionType.Walk),
                    new AgentSegment(new RGuideVector2(7f, 0.5f), ConnectionType.Walk)
                }),
        };

        private static Node[] BuildSequentialNodePath(params RGuideVector2[] positions)
        {
            var nodeStore = new NodeStore();
            var nodes = positions.Select(p => nodeStore.NewNode(p)).ToArray();

            for(var idx = 0; idx < nodes.Length; idx++)
            {
                var currentNode = nodes[idx];

                var prevIndex = idx - 1;
                if (prevIndex >= 0)
                {
                    var prevNode = nodes[prevIndex];
                    nodeStore.ConnectNodes(prevNode, currentNode, float.PositiveInfinity, ConnectionType.Walk, new NavTag());
                }

                var nextIndex = idx + 1;
                if (nextIndex < nodes.Length)
                {
                    var nextNode = nodes[nextIndex];
                    nodeStore.ConnectNodes(currentNode, nextNode, float.PositiveInfinity, ConnectionType.Walk, new NavTag());
                }
            }

            return nodes;
        }

        [Test]
        public void BuildPathFrom([ValueSource(nameof(BuildPathFromTestValues))] BuildPathFromParams values)
        {
            var agentPath = AgentSegmentPathBuilder.BuildPathFrom(values.Start, values.Target, values.Path, 0.1f, 30f, 1f);
            Assert.AreEqual(values.Result, agentPath);
        }
    }
}
