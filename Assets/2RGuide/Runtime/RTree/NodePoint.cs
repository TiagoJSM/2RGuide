using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using RTree;
using System.Collections;
using UnityEngine;

namespace RTree
{
    public class NodePoint : ISpatialData
    {
        private Node _node;

        public Envelope Envelope
        {
            get
            {
                return new Envelope(_node.Position.x, _node.Position.y);
            }
        }

        public NodePoint(Node node)
        {
            _node = node;
        }

        public Node Node => _node;
    }
}