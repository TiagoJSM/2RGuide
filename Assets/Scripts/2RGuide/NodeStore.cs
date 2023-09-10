using Assets.Scripts._2RGuide.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide
{
    public class NodeStore
    {
        private List<Node> _nodes = new List<Node>();

        public Node NewNode(Vector2 position)
        {
            if (!Contains(position))
            {
                var node = new Node() { Position = position };
                _nodes.Add(node);
                return node;
            }
            return null;
        }

        public Node Get(Vector2 position)
        {
            return _nodes.FirstOrDefault(n => n.Position == position);
        }

        public bool Contains(Vector2 position)
        {
            return _nodes.Any(n => n.Position.Approximately(position));
        }

        public Node[] ToArray()
        {
            return _nodes.ToArray();
        }
    }
}