﻿using Assets._2RGuide.Runtime.Helpers;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System;
using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Math;

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

    public class GuideAgent : MonoBehaviour, IAgentOperationsContext
    {
        public struct AgentSegment
        {
            public Vector2 position;
            public ConnectionType connectionType;
        }

        public struct PathfindingRequest
        {
            public Vector2? destinationPoint;
            public GameObject destinationTarget;

            public Vector2 DestinationPosition
            {
                get
                {
                    if(destinationPoint.HasValue)
                    {
                        return destinationPoint.Value;
                    }
                    return destinationTarget.transform.position;
                }
            }
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

        private AgentOperations _agentOperations;

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

        public Vector2 DesiredMovement => _agentOperations.DesiredMovement;
        public ConnectionType? CurrentConnectionType => _agentOperations.CurrentConnectionType;
        public Vector2? CurrentTargetPosition => _agentOperations.CurrentTargetPosition;
        public AgentStatus Status => _agentOperations.Status;
        public PathStatus CurrentPathStatus => _agentOperations.CurrentPathStatus;
        public bool IsSearchingForPath => _agentOperations.IsSearchingForPath;
        public Vector2 Position => transform.position;

        public void SetDestination(Vector2 destination)
        {
            _agentOperations.SetDestination(destination);
        }

        public void SetDestination(GameObject destination)
        {
            _agentOperations.SetDestination(destination);
        }

        public void CancelPathFinding()
        {
            _agentOperations.CancelPathFinding();
        }

        public void CompleteCurrentSegment()
        {
            _agentOperations.CompleteCurrentSegment();
        }

        public TaskCoroutine<GuideAgentHelper.PathfindingResult> FindPath(
            Vector2 start,
            Vector2 end,
            float maxHeight,
            float maxSlopeDegrees,
            ConnectionType allowedConnectionTypes,
            float pathfindingMaxDistance,
            float segmentProximityMaxDistance,
            NavTag[] navTagCapable,
            float stepHeight,
            ConnectionTypeMultipliers connectionMultipliers)
        {
            return TaskCoroutine<GuideAgentHelper.PathfindingResult>.Run(() =>
                GuideAgentHelper.PathfindingTask(
                    start,
                    end,
                    _height,
                    _maxSlopeDegrees,
                    _allowedConnectionTypes,
                    _pathfindingMaxDistance,
                    segmentProximityMaxDistance,
                    _navTagCapable,
                    _stepHeight,
                    _connectionMultipliers));
        }

        private void Awake()
        {
            _agentOperations =
                new AgentOperations(
                    this,
                    Nav2RGuideSettings.Load(),
                    _speed,
                    _height,
                    _maxSlopeDegrees,
                    _baseOffset,
                    _proximityThreshold,
                    _allowedConnectionTypes,
                    _pathfindingMaxDistance,
                    _navTagCapable,
                    _stepHeight,
                    _connectionMultipliers);
        }

        private void Start()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                return;
            }

            var navWorld = NavWorldReference.Instance.NavWorld;

            if (navWorld == null)
            {
                Debug.LogError($"NavWorld not present in scene, can't use {nameof(GuideAgent)}");
                return;
            }
        }

        void Update()
        {
            _agentOperations.Update();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_agentOperations.ReferencePosition, _proximityThreshold);
        }

#if UNITY_EDITOR
        private static float LineThickness => EditorGUIUtility.pixelsPerPoint * 3;

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy, typeof(GuideAgent))]
        private static void RenderCustomGizmo(GuideAgent objectTransform, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var path = objectTransform._agentOperations.Path;
            if (path == null || objectTransform._agentOperations.Settings == null)
            {
                return;
            }

            var start = objectTransform._agentOperations.ReferencePosition;
            var debugPathVerticalOffset = new Vector2(0, objectTransform._agentOperations.Settings.AgentDebugPathVerticalOffset);

            for (var idx = objectTransform._agentOperations.TargetPathIndex; idx < path.Length; idx++)
            {
                Handles.color = Gizmos.color = Color.green;
                Handles.DrawLine(start + debugPathVerticalOffset, path[idx].position + debugPathVerticalOffset, LineThickness);
                Gizmos.DrawWireSphere(path[idx].position + debugPathVerticalOffset, objectTransform._agentOperations.Settings.AgentTargetPositionDebugSphereRadius);
                start = path[idx].position;
            }
        }
#endif
    }
}