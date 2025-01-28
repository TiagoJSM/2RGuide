using Assets._2RGuide.Runtime.Helpers;
using UnityEngine;
using System;
using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Math;
using System.Collections;
using UnityEngine.UIElements;



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

        private class GuideAgentOperationsContext : IAgentOperationsContext
        {
            private GuideAgent _agent;
            public GuideAgentOperationsContext(GuideAgent agent)
            {
                _agent = agent;
            }

            public NavWorldManager NavWorldManager => _agent._navWorldManager;

            public Vector2 Position => _agent.transform.position;

            public TaskCoroutine<PathfindingTask.PathfindingResult> RunPathfinding(
                RGuideVector2 start, 
                RGuideVector2 end, 
                float maxHeight, 
                float maxSlopeDegrees, 
                ConnectionType allowedConnectionTypes, 
                float pathfindingMaxDistance, 
                float segmentProximityMaxDistance, 
                NavTag[] navTagCapable, 
                float stepHeight, 
                ConnectionTypeMultipliers connectionMultipliers)
            {
                return _agent.RunPathfinding(
                    start,
                    end,
                    maxHeight,
                    maxSlopeDegrees,
                    allowedConnectionTypes,
                    pathfindingMaxDistance,
                    segmentProximityMaxDistance,
                    navTagCapable,
                    stepHeight,
                    connectionMultipliers);
            }

            public Coroutine StartCoroutine(IEnumerator routine)
            {
                return _agent.StartCoroutine(routine);
            }

            public void StopCoroutine(Coroutine routine)
            {
                _agent.StopCoroutine(routine);
            }
        }

        private NavWorldManager _navWorldManager;
        private GuideAgentOperationsContext _operationsContext;
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

        public Vector2 DesiredMovement => _agentOperations.DesiredMovement.ToVector2();
        public ConnectionType? CurrentConnectionType => _agentOperations.CurrentConnectionType;
        public Vector2? CurrentTargetPosition => _agentOperations.CurrentTargetPosition?.ToVector2();
        public bool? IsCurrentSegmentStep => _agentOperations.IsCurrentSegmentStep;
        public AgentStatus Status => _agentOperations.Status;
        public PathStatus CurrentPathStatus => _agentOperations.CurrentPathStatus;
        public bool IsSearchingForPath => _agentOperations.IsSearchingForPath;
        public float Height => _height;
        public float MaxSlopeDegrees => _maxSlopeDegrees;
        public ConnectionType AllowedConnectionTypes => _allowedConnectionTypes;
        public float PathfindingMaxDistance => _pathfindingMaxDistance;
        public NavTag[] NavTagCapable => _navTagCapable;
        public float StepHeight => _stepHeight;
        public ConnectionTypeMultipliers ConnectionMultipliers => _connectionMultipliers;

        public void SetDestination(Vector2 destination, bool allowIncompletePath, float targetRange)
        {
            if (IsTargetInRange(targetRange, destination))
            {
                _agentOperations.CancelPathFinding();
            }
            else
            {
                _agentOperations.SetDestination(new RGuideVector2(destination), allowIncompletePath, targetRange);
            }
        }

        public void SetDestination(GameObject destination, bool allowIncompletePath, float targetRange)
        {
            if (IsTargetInRange(targetRange, destination.transform.position))
            {
                _agentOperations.CancelPathFinding();
            }
            else
            {
                _agentOperations.SetDestination(destination, allowIncompletePath, targetRange);
            }
        }

        public void CancelPathFinding()
        {
            _agentOperations.CancelPathFinding();
        }

        public void CompleteCurrentSegment()
        {
            _agentOperations.CompleteCurrentSegment();
        }

        public IEnumerator FindPath(Vector2 position, bool allowIncompletePath, float targetRange)
        {
            SetDestination(position, allowIncompletePath, targetRange);

            if (Status == AgentStatus.Iddle)
            {
                yield break;
            }

            yield return new WaitWhile(() => Status == AgentStatus.Busy);
        }

        public IEnumerator FindPath(GameObject target, bool allowIncompletePath, float targetRange)
        {
            SetDestination(target, allowIncompletePath, targetRange);

            if (Status == AgentStatus.Iddle)
            {
                yield break;
            }

            yield return new WaitWhile(() => Status == AgentStatus.Busy);
        }

        private TaskCoroutine<PathfindingTask.PathfindingResult> RunPathfinding(
            RGuideVector2 start,
            RGuideVector2 end,
            float maxHeight,
            float maxSlopeDegrees,
            ConnectionType allowedConnectionTypes,
            float pathfindingMaxDistance,
            float segmentProximityMaxDistance,
            NavTag[] navTagCapable,
            float stepHeight,
            ConnectionTypeMultipliers connectionMultipliers)
        {
            return _navWorldManager.RunPathfinding(
                gameObject,
                start,
                end,
                maxHeight,
                maxSlopeDegrees,
                allowedConnectionTypes,
                pathfindingMaxDistance,
                segmentProximityMaxDistance,
                navTagCapable,
                stepHeight,
                connectionMultipliers);
        }

        private bool IsTargetInRange(float targetRange, Vector3 targetPosition)
        {
            return Vector2.Distance(transform.position, targetPosition) < targetRange;
        }

        private void Awake()
        {
            _navWorldManager = NavWorldManager.Instance;
            _operationsContext = new GuideAgentOperationsContext(this);
            _agentOperations =
                new AgentOperations(
                    _operationsContext,
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

            var navWorld = _navWorldManager.NavWorld;

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
            Gizmos.DrawWireSphere(_agentOperations.ReferencePosition.ToVector2(), _proximityThreshold);
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
                Handles.DrawLine(start.ToVector2() + debugPathVerticalOffset, path[idx].Position.ToVector2() + debugPathVerticalOffset, LineThickness);
                Gizmos.DrawWireSphere(path[idx].Position.ToVector2() + debugPathVerticalOffset, objectTransform._agentOperations.Settings.AgentTargetPositionDebugSphereRadius);
                start = path[idx].Position;
            }
        }
#endif
    }
}