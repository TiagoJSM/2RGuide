using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide
{
    public class NavWorld : MonoBehaviour
    {
        private List<Node> _nodes;

        public Segment[] segments;

        public IEnumerable<Node> Nodes => _nodes;

        private void Awake()
        {
            _nodes = new List<Node>();

            BuildNodes();
            CreateConnections();
        }

        private void BuildNodes()
        {
            foreach (var segment in segments)
            {
                var n1 = new Node() { Position = segment.p1 };
                var n2 = new Node() { Position = segment.p2 };

                if (!_nodes.Contains(n1))
                {
                    _nodes.Add(n1);
                }
                if (!_nodes.Contains(n2))
                {
                    _nodes.Add(n2);
                }
            }
        }

        private void CreateConnections()
        {
            foreach (var segment in segments)
            {
                var n1 = _nodes.First(n => n.Position == segment.p1);
                var n2 = _nodes.First(n => n.Position == segment.p2);

                n1.Connections.Add(n2);
                n2.Connections.Add(n1);
            }
        }
    }
}