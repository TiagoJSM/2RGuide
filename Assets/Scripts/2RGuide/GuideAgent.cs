﻿using Assets.Scripts._2RGuide.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static Assets.Scripts._2RGuide.GuideAgent;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts._2RGuide
{
    public class GuideAgent : MonoBehaviour
    {
        public enum AgentStatus
        {
            Iddle,
            Busy,
            Moving
        }

        private Node[] _allNodes;
        private Vector2[] _path;
        private int _targetPathIndex;

        private Vector2? _currentDestination;
        private Vector2? _desiredDestination;
        private AgentStatus _agentStatus = AgentStatus.Iddle;
        private Coroutine _coroutine;

        [SerializeField]
        private float _speed;

        public Vector2 DesiredMovement { get; private set; }

        public void SetDestination(Vector2 destination)
        {
            _desiredDestination = destination;
        }

        public void CancelPathFinding()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            _currentDestination = null;
            _agentStatus = AgentStatus.Iddle;
            _path = null;
        }

        private void Start()
        {
            var navWorld = NavWorldReference.Instance.NavWorld;

            if (navWorld == null)
            {
                Debug.LogError($"NavWorld not present in scene, can't use {nameof(GuideAgent)}");
                return;
            }

            _allNodes = navWorld.nodes;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_currentDestination.HasValue && _desiredDestination.HasValue)
            {
                CancelPathFinding();
                _currentDestination = _desiredDestination;
                _desiredDestination = null;
                var startN = _allNodes.MinBy(n => Vector2.Distance(transform.position, n.Position));
                var endN = _allNodes.MinBy(n => Vector2.Distance(_currentDestination.Value, n.Position));
                _coroutine = StartCoroutine(FindPath(startN, endN));
            }
            
            Move();
        }

        private void Move()
        {
            if (_path == null)
            {
                return;
            }

            _agentStatus = AgentStatus.Moving;

            var step = _speed * Time.deltaTime;

            if (Approximatelly(transform.position, _path[_targetPathIndex]))
            {
                _targetPathIndex++;
                if (_targetPathIndex >= _path.Length)
                {
                    _desiredDestination = null;
                    _agentStatus = AgentStatus.Iddle;
                    _path = null;
                }
            }

            if (_targetPathIndex < _path.Length)
            {
                DesiredMovement = Vector2.MoveTowards(transform.position, _path[_targetPathIndex], step) - (Vector2)transform.position;
            }
        }

        private bool Approximatelly(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }

        private IEnumerator FindPath(Node start, Node end)
        {
            _agentStatus = AgentStatus.Busy;
            var pathfindingTask = Task.Run(() => AStar.Resolve(start, end));

            while (!pathfindingTask.IsCompleted)
            {
                yield return null;
            }

            var path = pathfindingTask.Result;
            _targetPathIndex = 0;

            if (path.Length > 1)
            {
                var connection = path.ElementAt(0).Connections.First(c => c.node.Equals(path.ElementAt(1)));
                if (connection.segment.OnSegment(transform.position))
                {
                    _targetPathIndex = 1;
                }
            }

            _coroutine = null;
            _path = path.Select(n => n.Position).ToArray();
        }
    }
}