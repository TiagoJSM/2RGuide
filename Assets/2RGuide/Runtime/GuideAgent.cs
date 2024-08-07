﻿using Assets._2RGuide.Runtime.Helpers;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets._2RGuide.Runtime
{
    [Serializable]
    public class ConnectionTypeMultipliers
    {
        [SerializeField]
        private float _walk = 1.0f;
        [SerializeField]
        private float _drop = 1.0f;
        [SerializeField]
        private float _jump = 1.0f;
        [SerializeField]
        private float _oneWayPlatformJump = 1.0f;
        [SerializeField]
        private float _oneWayPlatformDrop = 1.0f;

        public float Walk => _walk;
        public float Drop => _drop;
        public float Jump => _jump;
        public float OneWayPlatformJump => _oneWayPlatformJump;
        public float OneWayPlatformDrop => _oneWayPlatformDrop;
    }

    public class GuideAgent : MonoBehaviour
    {
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
        private Nav2RGuideSettings _settings;

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
        [SerializeField]
        private NavTag[] _navTagCapable;
        [SerializeField]
        private float _stepHeight;
        [SerializeField]
        private ConnectionTypeMultipliers _connectionMultipliers;

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
            ResetInternalState();
        }

        public void CompleteCurrentSegment()
        {
            if (_path == null)
            {
                return;
            }

            _targetPathIndex++;
            if (_targetPathIndex >= _path.Length)
            {
                ResetInternalState();
            }
        }

        private void Start()
        {
            var navWorld = NavWorldReference.Instance.NavWorld;
            _settings = Nav2RGuideSettings.Load();

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
            var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;

            var pathfindingTask = Task.Run(() =>
            {
                return GuideAgentHelper.PathfindingTask(start, end, _height, _maxSlopeDegrees, _allowedConnectionTypes, _pathfindingMaxDistance, segmentProximityMaxDistance, _navTagCapable, _stepHeight, _connectionMultipliers);
            });

            while (!pathfindingTask.IsCompleted)
            {
                yield return null;
            }

            var result = pathfindingTask.Result;

            CurrentPathStatus = result.pathStatus;

            if (result.segmentPath == null)
            {
                _coroutine = null;
                _path = result.segmentPath;
                _agentStatus = AgentStatus.Iddle;
                yield break;
            }

            _agentStatus = AgentStatus.Moving;
            _targetPathIndex = 0;

            if (result.segmentPath.Length < 2)
            {
                _coroutine = null;
                _path = null;
                _agentStatus = AgentStatus.Iddle;
                yield break;
            }

            _coroutine = null;
            _path = result.segmentPath;
        }

        private void ResetInternalState()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _currentDestination = null;
            _desiredDestination = null;
            _agentStatus = AgentStatus.Iddle;
            //CurrentPathStatus = PathStatus.Invalid;
            _path = null;
            DesiredMovement = Vector2.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ReferencePosition, ProximityThreshold);
        }

#if UNITY_EDITOR
        private static float LineThickness => EditorGUIUtility.pixelsPerPoint * 3;

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy, typeof(GuideAgent))]
        private static void RenderCustomGizmo(GuideAgent objectTransform, GizmoType gizmoType)
        {
            if (objectTransform._path == null || objectTransform._settings == null)
            {
                return;
            }

            var start = objectTransform.ReferencePosition;
            var debugPathVerticalOffset = new Vector2(0, objectTransform._settings.AgentDebugPathVerticalOffset);

            for (var idx = objectTransform._targetPathIndex; idx < objectTransform._path.Length; idx++)
            {
                Handles.color = Gizmos.color = Color.green;
                Handles.DrawLine(start + debugPathVerticalOffset, objectTransform._path[idx].position + debugPathVerticalOffset, LineThickness);
                Gizmos.DrawWireSphere(objectTransform._path[idx].position + debugPathVerticalOffset, objectTransform._settings.AgentTargetPositionDebugSphereRadius);
                start = objectTransform._path[idx].position;
            }
        }
#endif
    }
}