using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    [Flags]
    public enum ConnectionType
    {
        None = 0,
        Walk = 1 << 1,
        Drop = 1 << 2,
        Jump = 1 << 3,
        OneWayPlatformJump = 1 << 4,
        OneWayPlatformDrop = 1 << 5,
        All = Walk | Drop | Jump | OneWayPlatformJump | OneWayPlatformDrop
    }

    [Serializable]
    public struct NodeConnection
    {
        [HideInInspector]
        [SerializeReference]
        private NodeStore _nodeStore;
        [SerializeField]
        private int _nodeIndex;
        [SerializeField]
        private ConnectionType _connectionType;
        [SerializeField]
        private LineSegment2D _segment;
        [SerializeField]
        private float _maxHeight;
        [SerializeField]
        private NavTag _navTag;

        public Node Node => _nodeStore.Get(_nodeIndex);
        public ConnectionType ConnectionType => _connectionType;
        public LineSegment2D Segment => _segment;
        public float MaxHeight => _maxHeight;
        public NavTag NavTag => _navTag;

        public NodeConnection(
            NodeStore store,
            int nodeIndex,
            ConnectionType connectionType,
            LineSegment2D segment,
            float maxHeight,
            NavTag navTags)
        {
            _nodeStore = store;
            _nodeIndex = nodeIndex;
            _connectionType = connectionType;
            _segment = segment;
            _maxHeight = maxHeight;
            _navTag = navTags;
        }
    }

    [Serializable]
    public class Node
    {
        [HideInInspector]
        [SerializeReference]
        private NodeStore _nodeStore;
        [SerializeField]
        private int _nodeIndex;

        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private List<NodeConnection> _connections;

        public int NodeIndex => _nodeIndex;
        public Vector2 Position => _position;
        public IEnumerable<NodeConnection> Connections => _connections;

        public Node() { }

        public Node(
            NodeStore nodeStore,
            Vector2 position,
            int nodeIndex)
        {
            _nodeStore = nodeStore;
            _position = position;
            _nodeIndex = nodeIndex;
            _connections = new List<NodeConnection>();
        }

        public bool AddConnection(ConnectionType connectionType, Node other, LineSegment2D segment, float maxHeight, NavTag navTag)
        {
            var hasSegment = _connections.Any(c => c.Segment.IsCoincident(segment));
            if (!hasSegment)
            {
                var connection = new NodeConnection(_nodeStore, other._nodeIndex, connectionType, segment, maxHeight, navTag);
                _connections.Add(connection);
            }

            return !hasSegment;
        }

        public NodeConnection? ConnectionWith(Node n)
        {
            foreach(var connection in Connections) 
            {
                if (connection.Node.Equals(n))
                {
                    return connection;
                }
            }

            return null;
        }

        public void RemoveConnectionWith(Node n)
        {
            _connections.RemoveAll(nc => nc.Node == n);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Node other)
            {
                return other.Position.Approximately(Position);
            }
            return false;
        }
    }

    public class NodeConnectionEqualityComparer : IEqualityComparer<NodeConnection>
    {
        public bool Equals(NodeConnection x, NodeConnection y)
        {
            return x.Segment.IsCoincident(y.Segment);
        }

        public int GetHashCode(NodeConnection obj)
        {
            return obj.Segment.P1.GetHashCode() + obj.Segment.P2.GetHashCode();
        }
    }

    [Serializable]
    public class NodeStore
    {
        [SerializeField]
        private List<Node> _nodes = new List<Node>();

        public Node NewNode(Vector2 position)
        {
            if (!Contains(position))
            {
                var node = new Node(this, position, _nodes.Count);
                _nodes.Add(node);
                return node;
            }
            return null;
        }

        public Node NewNodeOrExisting(Vector2 position)
        {
            return NewNode(position) ?? Get(position);
        }

        public Node SplitSegmentAt(LineSegment2D segment, Vector2 position)
        {
            var splitNode = Get(position);

            if (splitNode != null)
            {
                return splitNode;
            }

            var connectedNode1 = Get(segment.P1);
            var connectedNode2 = Get(segment.P2);
            var connection = connectedNode1.ConnectionWith(connectedNode2);

            if (connection == null)
            {
                return null;
            }

            var maxHeight = connection.Value.MaxHeight;
            var navTag = connection.Value.NavTag;
            var connectionType = connection.Value.ConnectionType;

            splitNode = NewNode(position);

            connectedNode1.RemoveConnectionWith(connectedNode2);
            connectedNode2.RemoveConnectionWith(connectedNode1);

            ConnectNodes(connectedNode1, splitNode, maxHeight, connectionType, navTag);
            ConnectNodes(splitNode, connectedNode2, maxHeight, connectionType, navTag);

            return splitNode;
        }

        public Node Get(int nodeIndex)
        {
            return _nodes[nodeIndex];
        }

        public Node Get(Vector2 position)
        {
            return _nodes.FirstOrDefault(n => n.Position.Approximately(position));
        }

        public Node ClosestTo(Vector2 position)
        {
            return _nodes.MinBy(n => Vector2.Distance(position, n.Position));
        }

        public bool Contains(Vector2 position)
        {
            return _nodes.Any(n => n.Position.Approximately(position));
        }

        public Node[] GetNodes()
        {
            return _nodes.ToArray();
        }

        public int NodeCount => _nodes.Count;

        public LineSegment2D ConnectNodes(Node node1, Node node2, float maxHeight, ConnectionType connectionType, NavTag navTag)
        {
            var s = new LineSegment2D(node1.Position, node2.Position);

            node1.AddConnection(connectionType, node2, s, maxHeight, navTag);
            node2.AddConnection(connectionType, node1, s, maxHeight, navTag);

            return s;
        }

        public NodeConnection[] GetUniqueNodeConnections()
        {
            var connections = _nodes.SelectMany(n => n.Connections).Distinct(new NodeConnectionEqualityComparer());
            return connections.ToArray();
        }
    }
}