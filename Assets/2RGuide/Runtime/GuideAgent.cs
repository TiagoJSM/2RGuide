﻿using _2RGuide.Helpers;
using Assets._2RGuide.Runtime.Helpers;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace _2RGuide
{
    public class GuideAgent : MonoBehaviour
    {
        private struct PathfindingResult
        {
            public PathStatus pathStatus;
            public Node[] nodes;
        }

        public struct AgentSegment
        {
            public Vector2 position;
            public ConnectionType connectionType;
        }

        public enum AgentStatus
        {
            Iddle,
            Busy,
            Moving
        }

        public enum PathStatus
        {
            Invalid,
            Incomplete,
            Complete
        }

        private AgentSegment[] _path;
        private int _targetPathIndex;

        private Vector2? _currentDestination;
        private Vector2? _desiredDestination;
        private AgentStatus _agentStatus = AgentStatus.Iddle;
        private Coroutine _coroutine;

        [SerializeField]
        private float _speed;
        [SerializeField]
        private float _height;
        [Range(0f, 90f)]
        [SerializeField]
        private float _maxSlopeDegrees;
        [SerializeField]
        private float _baseOffset;
        [SerializeField]
        private float _proximityThreshold;
        [SerializeField]
        private ConnectionType _allowedConnectionTypes = ConnectionType.All;
        [SerializeField]
        private float _pathfindingMaxDistance = float.PositiveInfinity;

        private Vector2 ReferencePosition => (Vector2)transform.position + new Vector2(0.0f, _baseOffset);
        private bool RequiresFindingNewPath => !_currentDestination.HasValue && _desiredDestination.HasValue;

        public Vector2 DesiredMovement { get; private set; }
        public ConnectionType? CurrentConnectionType => _path == null ? default(ConnectionType?) : _path[_targetPathIndex].connectionType;
        public Vector2? CurrentTargetPosition => _path == null ? default(Vector2?) : _path[_targetPathIndex].position;
        public AgentStatus Status => _agentStatus;
        public PathStatus CurrentPathStatus { get; private set; }
        public float BaseOffset
        {
            get => _baseOffset;
            set => _baseOffset = value;
        }
        public float ProximityThreshold
        {
            get => _proximityThreshold;
            set => _proximityThreshold = value;
        }

        public void SetDestination(Vector2 destination)
        {
            CancelPathFinding();
            _desiredDestination = destination;
            if (RequiresFindingNewPath)
            {
                _agentStatus = AgentStatus.Busy;
            }
        }

        public void CancelPathFinding()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            _currentDestination = null;
            _desiredDestination = null;
            _agentStatus = AgentStatus.Iddle;
            CurrentPathStatus = PathStatus.Invalid;
            _path = null;
        }

        public void CompleteCurrentSegment()
        {
            if(_path == null)
            {
                return;
            }

            _targetPathIndex++;
            if (_targetPathIndex >= _path.Length)
            {
                _desiredDestination = null;
                _agentStatus = AgentStatus.Iddle;
                CurrentPathStatus = PathStatus.Invalid;
                _path = null;
                DesiredMovement = Vector2.zero;
            }
        }

        private void Start()
        {
            var navWorld = NavWorldReference.Instance.NavWorld;

            if (navWorld == null)
            {
                Debug.LogError($"NavWorld not present in scene, can't use {nameof(GuideAgent)}");
                return;
            }
        }

        void Update()
        {
            if (RequiresFindingNewPath)
            {
                _currentDestination = _desiredDestination;
                _desiredDestination = null;
                _coroutine = StartCoroutine(FindPath(ReferencePosition, _currentDestination.Value));
            }
            
            Move();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            DrawPath();
#endif
        }

        private void Move()
        {
            if (_path == null)
            {
                return;
            }

            var step = _speed * Time.deltaTime;

            if (Vector2.Distance(ReferencePosition, _path[_targetPathIndex].position) <= ProximityThreshold)
            {
                CompleteCurrentSegment();
                if (_path == null)
                {
                    return;
                }
            }

            if (_targetPathIndex < _path.Length)
            {
                DesiredMovement = Vector2.MoveTowards(ReferencePosition, _path[_targetPathIndex].position, step) - ReferencePosition;
            }
        }

        private IEnumerator FindPath(Vector2 start, Vector2 end)
        {
            var pathfindingTask = Task.Run(() => 
            {
                var navWorld = NavWorldReference.Instance.NavWorld;
                var startN = navWorld.GetClosestNodeInSegment(start);
                var endN = navWorld.GetClosestNodeInSegment(end);
                var nodes = AStar.Resolve(startN, endN, _height, _maxSlopeDegrees, _allowedConnectionTypes, _pathfindingMaxDistance);
                var pathStatus = PathStatus.Invalid;

                if(nodes != null && nodes.Length > 0)
                {
                    pathStatus = nodes.Last().Equals(endN) ? PathStatus.Complete : PathStatus.Incomplete;
                }

                return new PathfindingResult()
                {
                    nodes = nodes,
                    pathStatus = pathStatus
                };
            });

            while (!pathfindingTask.IsCompleted)
            {
                yield return null;
            }

            var result = pathfindingTask.Result;
            var path = result.nodes;

            CurrentPathStatus = result.pathStatus;

            if (path == null)
            {
                _coroutine = null;
                _path = null;
                _agentStatus = AgentStatus.Iddle;
                yield break;
            }

            _agentStatus = AgentStatus.Moving;
            _targetPathIndex = 0;

            var segmentPath = AgentSegmentPathBuilder.BuildPathFrom(ReferencePosition, _currentDestination.Value, path);

            if (segmentPath.Length < 2)
            {
                _coroutine = null;
                _path = null;
                _agentStatus = AgentStatus.Iddle;
                yield break;
            }

            _coroutine = null;
            _path = segmentPath;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ReferencePosition, ProximityThreshold);
        }

#if UNITY_EDITOR
        private void DrawPath()
        {
            if (_path == null)
            {
                return;
            }

            var start = ReferencePosition;

            for (var idx = _targetPathIndex; idx < _path.Length; idx++)
            {
                Debug.DrawLine(start, _path[idx].position, Color.yellow);
                start = _path[idx].position;
            }
        }
#endif
    }
}