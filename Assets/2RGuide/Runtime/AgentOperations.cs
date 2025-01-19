using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using System.Collections;
using System.Linq;
using UnityEngine;
using static Assets._2RGuide.Runtime.GuideAgent;

namespace Assets._2RGuide.Runtime
{
    public enum AgentOperationsState
    {
        Iddle,
        MoveToPosition,
        FollowTarget,
    }

    public class AgentOperations
    {
        private enum TargetInSegmentState
        {
            InSegment,
            InOtherSegment,
            NotInAnySegment
        }

        private struct VerifyTargetInSegmentStateResult
        {
            public TargetInSegmentState targetInSegmentState;
            public RGuideVector2 positionInSegment;

            public VerifyTargetInSegmentStateResult(TargetInSegmentState state)
                : this(state, RGuideVector2.zero)
            {
            }

            public VerifyTargetInSegmentStateResult(TargetInSegmentState state, RGuideVector2 positionInSegment)
            {
                this.targetInSegmentState = state;
                this.positionInSegment = positionInSegment;
            }
        }

        public struct AgentSegment
        {
            public RGuideVector2 Position { get; set; }
            public ConnectionType ConnectionType { get; set; }
            public bool IsStep { get; set; }

            public AgentSegment(RGuideVector2 position, ConnectionType connectionType, bool isStep)
            {
                Position = position;
                ConnectionType = connectionType;
                IsStep = isStep;
            }
        }

        public struct PathfindingRequest
        {
            public RGuideVector2? destinationPoint;
            public GameObject destinationTarget;
            public bool allowIncompletePath;
            public float targetRange;

            public PathfindingRequest(RGuideVector2 destination, bool allowIncompletePath, float targetRange)
            {
                this.destinationPoint = destination;
                this.destinationTarget = null;
                this.allowIncompletePath = allowIncompletePath;
                this.targetRange = targetRange;
            }

            public PathfindingRequest(GameObject destination, bool allowIncompletePath, float targetRange)
            {
                this.destinationPoint = null;
                this.destinationTarget = destination;
                this.allowIncompletePath = allowIncompletePath;
                this.targetRange = targetRange;
            }

            public RGuideVector2 DestinationPosition
            {
                get
                {
                    if (destinationPoint.HasValue)
                    {
                        return destinationPoint.Value;
                    }
                    return new RGuideVector2(destinationTarget.transform.position);
                }
            }
        }

        private AgentOperationsState _agentOperationsState = AgentOperationsState.Iddle;
        private AgentSegment[] _path;
        private int _targetPathIndex;

        private NavSegment? _targetSegment;
        private PathfindingRequest? _currentPathFinding;
        private PathfindingRequest? _desiredPathFinding;

        private AgentStatus _agentStatus = AgentStatus.Iddle;
        private Coroutine _coroutine;
        private Nav2RGuideSettings _settings;
        private TaskCoroutine<PathfindingTask.PathfindingResult> _pathfindingTask;

        private IAgentOperationsContext _context;
        private float _speed;
        private float _height;
        private float _maxSlopeDegrees;
        private float _baseOffset;
        private float _proximityThreshold;
        private ConnectionType _allowedConnectionTypes;
        private float _pathfindingMaxDistance;
        private NavTag[] _navTagCapable;
        private float _stepHeight;
        private ConnectionTypeMultipliers _connectionMultipliers;

        private bool RequiresFindingNewPath => !_currentPathFinding.HasValue && _desiredPathFinding.HasValue;
        private bool HasArrivedAtTargetPosition
        {
            get
            {
                if(!_currentPathFinding.HasValue)
                {
                    return false;
                }

                var destinationPosition = _currentPathFinding.Value.DestinationPosition;

                return RGuideVector2.Distance(ReferencePosition, destinationPosition) <= _currentPathFinding.Value.targetRange;
            }
        }

        public Nav2RGuideSettings Settings => _settings;
        public RGuideVector2 ReferencePosition => new RGuideVector2(_context.Position) + new RGuideVector2(0.0f, _baseOffset);
        public RGuideVector2 DesiredMovement { get; private set; }
        public ConnectionType? CurrentConnectionType => _path == null ? default(ConnectionType?) : _path[_targetPathIndex].ConnectionType;
        public RGuideVector2? CurrentTargetPosition => _path == null ? default(RGuideVector2?) : _path[_targetPathIndex].Position;
        public bool? IsCurrentSegmentStep => _path == null ? default(bool?) : _path[_targetPathIndex].IsStep;
        public AgentStatus Status => _agentStatus;
        public PathStatus CurrentPathStatus { get; private set; }
        public AgentSegment[] Path => _path;
        public int TargetPathIndex => _targetPathIndex;
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
        public bool IsSearchingForPath { get; private set; }
        public bool HasReachedTargetPathPoint => _path != null && RGuideVector2.Distance(ReferencePosition, _path[_targetPathIndex].Position) <= ProximityThreshold;

        public AgentOperations(
            IAgentOperationsContext context,
            Nav2RGuideSettings settings,
            float speed,
            float height,
            float maxSlopeDegrees,
            float baseOffset,
            float proximityThreshold,
            ConnectionType allowedConnectionTypes,
            float pathfindingMaxDistance,
            NavTag[] navTagCapable,
            float stepHeight,
            ConnectionTypeMultipliers connectionMultipliers)
        {
            _context = context;
            _settings = settings;
            _speed = speed;
            _height = height;
            _maxSlopeDegrees = maxSlopeDegrees;
            _baseOffset = baseOffset;
            _proximityThreshold = proximityThreshold;
            _allowedConnectionTypes = allowedConnectionTypes;
            _pathfindingMaxDistance = pathfindingMaxDistance;
            _navTagCapable = navTagCapable;
            _stepHeight = stepHeight;
            _connectionMultipliers = connectionMultipliers;
        }

        public void SetDestination(RGuideVector2 destination, bool allowIncompletePath, float targetRange)
        {
            SetPathfindingRequest(new PathfindingRequest(destination, allowIncompletePath, targetRange));
        }

        public void SetDestination(GameObject destination, bool allowIncompletePath, float targetRange)
        {
            SetPathfindingRequest(new PathfindingRequest(destination, allowIncompletePath, targetRange));
        }

        public void CancelPathFinding()
        {
            ResetInternalState();
        }

        public void CompleteCurrentSegment()
        {
            switch (_agentOperationsState)
            {
                case AgentOperationsState.Iddle: break;
                case AgentOperationsState.MoveToPosition:
                    CompleteCurrentSegmentWhenMovingToPosition();
                    break;
                case AgentOperationsState.FollowTarget:
                    CompleteCurrentSegmentWhenFollowingTarget();
                    break;
            }
        }

        public void Update()
        {
            HandleStateMachine();
        }

        private void HandleStateMachine()
        {
            switch (_agentOperationsState)
            {
                case AgentOperationsState.Iddle: break;
                case AgentOperationsState.MoveToPosition:
                    UpdateMove();
                    break;
                case AgentOperationsState.FollowTarget:
                    UpdateMove();
                    break;
            }
        }

        //private void UpdateMoveToPosition()
        //{
        //    if (RequiresFindingNewPath)
        //    {
        //        _currentPathFinding = _desiredPathFinding;
        //        _desiredPathFinding = null;
        //        StartFindingPath(_currentPathFinding.Value);
        //    }

        //    CompleteSegmentIfArrivedAtTargetPathPoint();
        //    SetDesiredMovement();
        //}

        private void UpdateMove()
        {
            if (HasArrivedAtTargetPosition)                 // if has arrived at target positon cancel pathfinding
            {
                CancelPathFinding();
                return;
            }
            if (RequiresFindingNewPath)
            {
                _currentPathFinding = _desiredPathFinding;
                _desiredPathFinding = null;
                StartFindingPath(_currentPathFinding.Value);
            }

            UpdatePathSegment();                            // first update path segments in case target moved away
            CompleteSegmentIfArrivedAtTargetPathPoint();    // second complete current segments if context has reached target position
            SetDesiredMovement();                           // then set the desired movement values to be handled by whoever handles the movement
        }

        private void CompleteSegmentIfArrivedAtTargetPathPoint()
        {
            while (HasReachedTargetPathPoint) // in case path points are very close
            {
                CompleteCurrentSegment();
            }
        }

        private void CompleteCurrentSegmentWhenFollowingTarget()
        {
            if (_path == null)
            {
                return;
            }

            _targetPathIndex++;
            if (_targetPathIndex >= _path.Length)
            {
                if(IsSearchingForPath)
                {
                    _path = null;
                    DesiredMovement = RGuideVector2.zero;
                }
                else
                {
                    ResetInternalState();
                }
            }
        }

        private void CompleteCurrentSegmentWhenMovingToPosition()
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

        private void SetPathfindingRequest(PathfindingRequest request)
        {
            CancelPathFinding();
            _desiredPathFinding = request;
            if (RequiresFindingNewPath)
            {
                _agentOperationsState = 
                    request.destinationPoint.HasValue 
                    ? AgentOperationsState.MoveToPosition 
                    : AgentOperationsState.FollowTarget;
                _agentStatus = AgentStatus.Busy;
            }
        }

        private void UpdatePathSegment()
        {
            if (!_currentPathFinding.HasValue || _currentPathFinding.Value.destinationTarget == null)
            {
                return;
            }
            if (_agentStatus != AgentStatus.Moving)
            {
                return;
            }
            if (CurrentPathStatus != PathStatus.Complete)
            {
                return;
            }

            var isTargetInSameSegment = VerifyTargetInSegmentState();
            
            if (isTargetInSameSegment.targetInSegmentState == TargetInSegmentState.InSegment)
            {
                _path[_path.Length - 1].Position = isTargetInSameSegment.positionInSegment;
            }
            else if(isTargetInSameSegment.targetInSegmentState == TargetInSegmentState.InOtherSegment)
            {
                _agentStatus = AgentStatus.Busy;
                StartFindingPath(_currentPathFinding.Value);
            }
        }

        private void SetDesiredMovement()
        {
            if (_path == null)
            {
                return;
            }

            var step = _speed * Time.deltaTime;

            if (_targetPathIndex < _path.Length)
            {
                DesiredMovement = RGuideVector2.MoveTowards(ReferencePosition, _path[_targetPathIndex].Position, step) - ReferencePosition;
            }
        }

        private void StartFindingPath(PathfindingRequest pathfindingRequest)
        {
            var navWorld = _context.NavWorldManager.NavWorld;
            var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
            _targetSegment = navWorld.GetClosestNavSegment(pathfindingRequest.DestinationPosition, ConnectionType.Walk, segmentProximityMaxDistance);
            _coroutine = _context.StartCoroutine(FindPathRoutine(ReferencePosition, pathfindingRequest));
        }

        private IEnumerator FindPathRoutine(RGuideVector2 start, PathfindingRequest request)
        {
            var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
            var end = request.destinationPoint.HasValue ? request.destinationPoint.Value : new RGuideVector2(request.destinationTarget.transform.position);

            IsSearchingForPath = true;
            var taskCoroutine = _context.FindPath(
                start,
                end,
                _height,
                _maxSlopeDegrees,
                _allowedConnectionTypes,
                _pathfindingMaxDistance,
                segmentProximityMaxDistance,
                _navTagCapable,
                _stepHeight,
                _connectionMultipliers);

            yield return taskCoroutine;
            IsSearchingForPath = false;

            HandleTaskIfError(taskCoroutine);

            var result = taskCoroutine.Result;

            CurrentPathStatus = result.pathStatus;
            _targetPathIndex = 0;

            if (CurrentPathStatus == PathStatus.Invalid || CurrentPathStatus == PathStatus.Incomplete && !request.allowIncompletePath || result.segmentPath.Length < 2)
            {
                _coroutine = null;
                _path = null;
                _agentStatus = AgentStatus.Iddle;
                _agentOperationsState = AgentOperationsState.Iddle;
                yield break;
            }

            _agentStatus = AgentStatus.Moving;

            _coroutine = null;
            _path = result.segmentPath;
        }

        private void HandleTaskIfError(TaskCoroutine<PathfindingTask.PathfindingResult> taskCoroutine)
        {
            if (taskCoroutine.Exception != null)
            {
                Debug.LogException(taskCoroutine.Exception);
                ResetInternalState();
                throw taskCoroutine.Exception;
            }
        }

        private void ResetInternalState()
        {
            if (_coroutine != null)
            {
                _context.StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _desiredPathFinding = null;
            _currentPathFinding = null;
            _agentStatus = AgentStatus.Iddle;
            _agentOperationsState = AgentOperationsState.Iddle;
            _path = null;
            DesiredMovement = RGuideVector2.zero;
        }

        private VerifyTargetInSegmentStateResult VerifyTargetInSegmentState()
        {
            var navWorld = _context.NavWorldManager.NavWorld;
            var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
            var position = _currentPathFinding.Value.destinationTarget.transform.position;
            var navSegment = navWorld.GetClosestNavSegment(new RGuideVector2(position.x, position.y), ConnectionType.Walk, segmentProximityMaxDistance);
            if(!navSegment)
            {
                return new VerifyTargetInSegmentStateResult(TargetInSegmentState.NotInAnySegment);
            }

            var closestPositionOnSegment = navSegment.segment.ClosestPointOnLine(new RGuideVector2(position));

            var state = navSegment.segment.IsCoincident(_targetSegment.Value.segment) 
                ? TargetInSegmentState.InSegment
                : TargetInSegmentState.InOtherSegment;

            return new VerifyTargetInSegmentStateResult(state, closestPositionOnSegment);
        }
    }
}