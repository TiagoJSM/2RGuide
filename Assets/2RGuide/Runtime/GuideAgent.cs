using Assets._2RGuide.Runtime.Helpers;
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
        //private AgentSegment[] _path;
        //private int _targetPathIndex;

        //private NavSegment? _targetSegment;
        //private PathfindingRequest? _currentPathFinding;
        //private PathfindingRequest? _desiredPathFinding;

        //private AgentStatus _agentStatus = AgentStatus.Iddle;
        //private Coroutine _coroutine;
        //private Nav2RGuideSettings _settings;
        //private Task<GuideAgentHelper.PathfindingResult> _pathfindingTask;

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

        //private Vector2 ReferencePosition => (Vector2)transform.position + new Vector2(0.0f, _baseOffset);
        //private bool RequiresFindingNewPath => !_currentPathFinding.HasValue && _desiredPathFinding.HasValue;

        public Vector2 DesiredMovement => _agentOperations.DesiredMovement;//{ get; private set; }
        public ConnectionType? CurrentConnectionType => _agentOperations.CurrentConnectionType;//_path == null ? default(ConnectionType?) : _path[_targetPathIndex].connectionType;
        public Vector2? CurrentTargetPosition => _agentOperations.CurrentTargetPosition; //_path == null ? default(Vector2?) : _path[_targetPathIndex].position;
        public AgentStatus Status => _agentOperations.Status;//_agentStatus;
        public PathStatus CurrentPathStatus => _agentOperations.CurrentPathStatus;//{ get; private set; }
        public bool IsSearchingForPath => _agentOperations.IsSearchingForPath;//_pathfindingTask != null && !_pathfindingTask.IsCompleted;
        public Vector2 Position => transform.position;

        public void SetDestination(Vector2 destination)
        {
            _agentOperations.SetDestination(destination);
            //SetPathfindingRequest(new PathfindingRequest() { destinationPoint = destination });
        }

        public void SetDestination(GameObject destination)
        {
            _agentOperations.SetDestination(destination);
            //SetPathfindingRequest(new PathfindingRequest() { destinationTarget = destination });
        }

        public void CancelPathFinding()
        {
            _agentOperations.CancelPathFinding();
            //ResetInternalState();
        }

        public void CompleteCurrentSegment()
        {
            _agentOperations.CompleteCurrentSegment();
            //if (_path == null)
            //{
            //    return;
            //}

            //_targetPathIndex++;
            //if (_targetPathIndex >= _path.Length)
            //{
            //    ResetInternalState();
            //}
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

            //_agentOperations = 
            //    new AgentOperations(
            //        this,
            //        Nav2RGuideSettings.Load(), 
            //        _speed, 
            //        _height, 
            //        _maxSlopeDegrees, 
            //        _baseOffset, 
            //        _proximityThreshold, 
            //        _allowedConnectionTypes, 
            //        _pathfindingMaxDistance, 
            //        _navTagCapable, 
            //        _stepHeight, 
            //        _connectionMultipliers);
        }

        void Update()
        {
            _agentOperations.Update();
            //if (RequiresFindingNewPath)
            //{
            //    var navWorld = NavWorldReference.Instance.NavWorld;
            //    var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
            //    _currentPathFinding = _desiredPathFinding;
            //    _targetSegment = navWorld.GetClosestNavSegment(_currentPathFinding.Value.DestinationPosition, ConnectionType.Walk, segmentProximityMaxDistance);
            //    _desiredPathFinding = null;
            //    _coroutine = StartCoroutine(FindPath(ReferencePosition, _currentPathFinding.Value));
            //}

            //UpdatePathSegment();
            //Move();
        }

        //private void SetPathfindingRequest(PathfindingRequest request)
        //{
        //    CancelPathFinding();
        //    _desiredPathFinding = request;
        //    if (RequiresFindingNewPath)
        //    {
        //        _agentStatus = AgentStatus.Busy;
        //    }
        //}

        //private void UpdatePathSegment()
        //{
        //    if(!_currentPathFinding.HasValue || _currentPathFinding.Value.destinationTarget == null)
        //    {
        //        return;
        //    }
        //    if(_agentStatus != AgentStatus.Moving)
        //    {
        //        return;
        //    }
        //    if(CurrentPathStatus != PathStatus.Complete)
        //    {
        //        return;
        //    }
        //    _path[_path.Length - 1].position = _currentPathFinding.Value.destinationTarget.transform.position;
        //}

        //private void Move()
        //{
        //    if (_path == null)
        //    {
        //        return;
        //    }

        //    if (!TargetInSameSegment())
        //    {
        //        FindPath(ReferencePosition, _currentPathFinding.Value);
        //    }

        //    var step = _speed * Time.deltaTime;

        //    if (Vector2.Distance(ReferencePosition, _path[_targetPathIndex].position) <= ProximityThreshold)
        //    {
        //        CompleteCurrentSegment();
        //        if (_path == null)
        //        {
        //            return;
        //        }
        //    }

        //    if (_targetPathIndex < _path.Length)
        //    {
        //        DesiredMovement = Vector2.MoveTowards(ReferencePosition, _path[_targetPathIndex].position, step) - ReferencePosition;
        //    }
        //}

        //private IEnumerator FindPath(Vector2 start, PathfindingRequest end)
        //{
        //    var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
        //    var endPos = end.destinationPoint.HasValue ? end.destinationPoint.Value : (Vector2)end.destinationTarget.transform.position;

        //    var taskCoroutine = TaskCoroutine<GuideAgentHelper.PathfindingResult>.Run(() => 
        //        GuideAgentHelper.PathfindingTask(
        //            start,
        //            endPos, 
        //            _height, 
        //            _maxSlopeDegrees, 
        //            _allowedConnectionTypes, 
        //            _pathfindingMaxDistance, 
        //            segmentProximityMaxDistance, 
        //            _navTagCapable, 
        //            _stepHeight, 
        //            _connectionMultipliers));

        //    //_pathfindingTask = Task.Run(() => 
        //    //    GuideAgentHelper.PathfindingTask(start, end.destinationPoint.Value, _height, _maxSlopeDegrees, _allowedConnectionTypes, _pathfindingMaxDistance, segmentProximityMaxDistance, _navTagCapable, _stepHeight, _connectionMultipliers));

        //    yield return taskCoroutine;

        //    if(taskCoroutine.Exception != null)
        //    {
        //        Debug.LogException(taskCoroutine.Exception);
        //        throw taskCoroutine.Exception;
        //    }

        //    var result = taskCoroutine.Result;

        //    CurrentPathStatus = result.pathStatus;

        //    if (result.segmentPath == null)
        //    {
        //        _coroutine = null;
        //        _path = result.segmentPath;
        //        _agentStatus = AgentStatus.Iddle;
        //        yield break;
        //    }

        //    _agentStatus = AgentStatus.Moving;
        //    _targetPathIndex = 0;

        //    if (result.segmentPath.Length < 2)
        //    {
        //        _coroutine = null;
        //        _path = null;
        //        _agentStatus = AgentStatus.Iddle;
        //        yield break;
        //    }

        //    _coroutine = null;
        //    _path = result.segmentPath;
        //}

        //private void ResetInternalState()
        //{
        //    if (_coroutine != null)
        //    {
        //        StopCoroutine(_coroutine);
        //        _coroutine = null;
        //    }

        //    _desiredPathFinding = null;
        //    _currentPathFinding = null;
        //    _agentStatus = AgentStatus.Iddle;
        //    _path = null;
        //    DesiredMovement = Vector2.zero;
        //}

        //private bool TargetInSameSegment()
        //{
        //    if(!_currentPathFinding.HasValue)
        //    {
        //        return false;
        //    }

        //    if(_currentPathFinding.Value.destinationTarget == null)
        //    {
        //        return false;
        //    }

        //    var navWorld = NavWorldReference.Instance.NavWorld;
        //    var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
        //    var navSegment = navWorld.GetClosestNavSegment(_currentPathFinding.Value.destinationTarget.transform.position, ConnectionType.Walk, segmentProximityMaxDistance);

        //    return navSegment.segment.IsCoincident(_targetSegment.Value.segment);
        //}

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