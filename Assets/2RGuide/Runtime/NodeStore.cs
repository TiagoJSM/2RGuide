using _2RGuide.Helpers;
using _2RGuide.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace _2RGuide
{
    [Flags]
    public enum ConnectionType
    {
        None = 0,
        Walk = 1 << 1,
        Drop = 1 << 2,
        Jump = 1 << 3,
        OneWayPlatformJump = 1 << 4,
        All = Walk | Drop | Jump | OneWayPlatformJump
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

        public Node Node => _nodeStore.Get(_nodeIndex);
        public ConnectionType ConnectionType => _connectionType;
        public LineSegment2D Segment => _segment;
        public float MaxHeight => _maxHeight;

        public NodeConnection(
            NodeStore store, 
            int nodeIndex, 
            ConnectionType connectionType, 
            LineSegment2D segment, 
            float maxHeight)
        {
            _nodeStore = store;
            _nodeIndex = nodeIndex;
            _connectionType = connectionType;
            _segment = segment;
            _maxHeight = maxHeight;
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
        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }
        public IEnumerable<NodeConnection> Connections => _connections;

        public Node() { }

        public Node(
            NodeStore nodeStore, 
            Vector2 position, 
            int nodeIndex)
        {
            _nodeStore = nodeStore;
            Position = position;
            _nodeIndex = nodeIndex;
            _connections = new List<NodeConnection>();
        }

        public bool AddConnection(ConnectionType connectionType, Node other, LineSegment2D segment, float maxHeight)
        {
            var hasSegment = _connections.Any(c => c.Segment.IsCoincident(segment));
            if (!hasSegment)
            {
                var connection = new NodeConnection(_nodeStore, other._nodeIndex, connectionType, segment, maxHeight);
                _connections.Add(connection);
            }

            return !hasSegment;
        }

        public NodeConnection? ConnectionWith(Node n)
        {
            var nc = Connections.FirstOrDefault(c => c.Node.Equals(n));
            return nc;
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

            if(splitNode != null)
            {
                return splitNode;
            }

            var connectedNode1 = Get(segment.P1);
            var connectedNode2 = Get(segment.P2);
            var connection = connectedNode1.ConnectionWith(connectedNode2);

            if(connection == null)
            {
                return null;
            }

            var maxHeight = connection.Value.MaxHeight;

            splitNode = NewNode(position);

            connectedNode1.RemoveConnectionWith(connectedNode2);
            connectedNode2.RemoveConnectionWith(connectedNode1);

            if (connectedNode1 != null)
            {
                splitNode.AddConnection(ConnectionType.Walk, connectedNode1, new LineSegment2D(segment.P1, splitNode.Position), maxHeight);
                connectedNode1.AddConnection(ConnectionType.Walk, splitNode, new LineSegment2D(splitNode.Position, segment.P1), maxHeight);
            }
            
            if (connectedNode2 != null)
            {
                splitNode.AddConnection(ConnectionType.Walk, connectedNode2, new LineSegment2D(splitNode.Position, segment.P2), maxHeight);
                connectedNode2.AddConnection(ConnectionType.Walk, splitNode, new LineSegment2D(segment.P2, splitNode.Position), maxHeight);
            }

            return splitNode;
        }

        public Node Get(int nodeIndex)
        {
            return _nodes[nodeIndex];
        }

        public Node Get(Vector2 position)
        {
            return _nodes.FirstOrDefault(n => n.Position == position);
        }

        public Node ClosestTo(Vector2 position)
        {
            return _nodes.MinBy(n => Vector2.Distance(position, n.Position));
        }

        public bool Contains(Vector2 position)
        {
            return _nodes.Any(n => n.Position.Approximately(position));
        }

        public Node[] ToArray()
        {
            return _nodes.ToArray();
        }

        public LineSegment2D ConnectNodes(Node node1, Node node2, float maxHeight, ConnectionType connectionType = ConnectionType.Walk)
        {
            var s = new LineSegment2D(node1.Position, node2.Position);

            node1.AddConnection(connectionType, node2, s, maxHeight);
            node2.AddConnection(connectionType, node1, s, maxHeight);

            return s;
        }

        public NodeConnection[] GetUniqueNodeConnections()
        {
            var connections = _nodes.SelectMany(n => n.Connections).Distinct(new NodeConnectionEqualityComparer());
            return connections.ToArray();
        }
    }
}