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
        //[SerializeReference]
        [NonSerialized]
        private NodeStore _nodeStore;
        public NodeStore NodeStore 
        {
            get => _nodeStore;
            set => _nodeStore = value;
        }
        [SerializeField]
        private int _nodeIndex;
        [SerializeField]
        private ConnectionType _connectionType;
        [SerializeField]
        private LineSegment2D _segment;
        [SerializeField]
        private float _maxHeight;

        public Node Node => NodeStore.Get(_nodeIndex);
        public ConnectionType ConnectionType => _connectionType;
        public LineSegment2D Segment => _segment;
        public float MaxHeight => _maxHeight;

        public NodeConnection(
            //NodeStore store, 
            int nodeIndex, 
            ConnectionType connectionType, 
            LineSegment2D segment, 
            float maxHeight)
        {
            _nodeStore = null;
            //NodeStore = store;
            _nodeIndex = nodeIndex;
            _connectionType = connectionType;
            _segment = segment;
            _maxHeight = maxHeight;
        }
    }

    [Serializable]
    public class Node
    {
        [NonSerialized]
        private NodeStore _nodeStore;
        //[SerializeReference]
        public NodeStore NodeStore 
        {
            get => _nodeStore;
            set
            {
                _nodeStore = value;
                for(var idx = 0; idx < _connections.Count; idx++)
                {
                    var connection = _connections[idx];
                    connection.NodeStore = _nodeStore;
                    _connections[idx] = connection;
                }
            }
        }
        [SerializeField]
        private int _nodeIndex;

        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private List<NodeConnection> _connections;

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }
        public IEnumerable<NodeConnection> Connections => _connections;

        public Node() { }

        public Node(
            //NodeStore nodeStore, 
            Vector2 position, 
            int nodeIndex)
        {
            //NodeStore = nodeStore;
            Position = position;
            _nodeIndex = nodeIndex;
            _connections = new List<NodeConnection>();
        }

        public bool AddConnection(ConnectionType connectionType, Node other, LineSegment2D segment, float maxHeight)
        {
            var hasSegment = _connections.Any(c => c.Segment.IsCoincident(segment));
            if (!hasSegment)
            {
                var connection = new NodeConnection(other._nodeIndex, connectionType, segment, maxHeight);
                connection.NodeStore = NodeStore;
                _connections.Add(connection);
            }

            return !hasSegment;
        }

        public NodeConnection? ConnectionWith(Node n)
        {
            var nc = Connections.FirstOrDefault(c => c.Node.Equals(n));
            return nc;
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

    [Serializable]
    public class NodeStore : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<Node> _nodes = new List<Node>();

        public Node NewNode(Vector2 position)
        {
            if (!Contains(position))
            {
                var node = new Node(position, _nodes.Count);
                node.NodeStore = this;
                _nodes.Add(node);
                return node;
            }
            return null;
        }

        public Node NewNodeOrExisting(Vector2 position)
        {
            return NewNode(position) ?? Get(position);
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

        public void ConnectWithNodesAtSegment(Node node, LineSegment2D segment, ConnectionType connectionType = ConnectionType.Walk)
        {
            var oneWayPlatN1 = Get(segment.P1);
            var oneWayPlatN2 = Get(segment.P2);

            var connection = oneWayPlatN1.ConnectionWith(oneWayPlatN2).Value;

            var s1 = new LineSegment2D(oneWayPlatN1.Position, node.Position);
            var s2 = new LineSegment2D(node.Position, oneWayPlatN2.Position);

            node.AddConnection(connectionType, oneWayPlatN1, s1, connection.MaxHeight);
            node.AddConnection(connectionType, oneWayPlatN2, s2, connection.MaxHeight);

            oneWayPlatN1.AddConnection(connectionType, node, s1, connection.MaxHeight);
            oneWayPlatN2.AddConnection(connectionType, node, s2, connection.MaxHeight);
        }

        public LineSegment2D ConnectNodes(Node node1, Node node2, float maxHeight, ConnectionType connectionType = ConnectionType.Walk)
        {
            var s = new LineSegment2D(node1.Position, node2.Position);

            node1.AddConnection(connectionType, node2, s, maxHeight);
            node2.AddConnection(connectionType, node1, s, maxHeight);

            return s;
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("after");

            foreach(var node in _nodes)
            {
                node.NodeStore = this;
            }
        }
    }
}