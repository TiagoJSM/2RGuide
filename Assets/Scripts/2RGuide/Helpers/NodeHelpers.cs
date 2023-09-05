using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class NodeHelpers
    {
        public static List<Node> BuildNodes(LineSegment2D[] segments)
        {
            var nodes = new List<Node>();

            foreach (var segment in segments)
            {
                var n1 = new Node() { Position = segment.P1 };
                var n2 = new Node() { Position = segment.P2 };

                if (!nodes.Contains(n1))
                {
                    nodes.Add(n1);
                }
                if (!nodes.Contains(n2))
                {
                    nodes.Add(n2);
                }
            }

            CreateNodeConnections(segments, nodes);

            return nodes;
        }

        private static void CreateNodeConnections(LineSegment2D[] segments, List<Node> result)
        {
            foreach (var segment in segments)
            {
                var n1 = result.First(n => n.Position == segment.P1);
                var n2 = result.First(n => n.Position == segment.P2);

                n1.Connections.Add(NodeConnection.Walk(n2, segment));
                n2.Connections.Add(NodeConnection.Walk(n1, segment));
            }
        }
    }
}