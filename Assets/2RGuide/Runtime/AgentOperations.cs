using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Helpers;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using static Assets._2RGuide.Runtime.GuideAgent;

namespace Assets._2RGuide.Runtime
{
    public class AgentOperations
    {
        private AgentSegment[] _path;
        private int _targetPathIndex;

        private NavSegment? _targetSegment;
        private PathfindingRequest? _currentPathFinding;
        private PathfindingRequest? _desiredPathFinding;

        private AgentStatus _agentStatus = AgentStatus.Iddle;
        private Coroutine _coroutine;
        private Nav2RGuideSettings _settings;
        private TaskCoroutine<GuideAgentHelper.PathfindingResult> _pathfindingTask;

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

        public Nav2RGuideSettings Settings => _settings;
        public Vector2 ReferencePosition => _context.Position + new Vector2(0.0f, _baseOffset);
        public Vector2 DesiredMovement { get; private set; }
        public ConnectionType? CurrentConnectionType => _path == null ? default(ConnectionType?) : _path[_targetPathIndex].connectionType;
        public Vector2? CurrentTargetPosition => _path == null ? default(Vector2?) : _path[_targetPathIndex].position;
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

        public void SetDestination(Vector2 destination)
        {
            SetPathfindingRequest(new PathfindingRequest() { destinationPoint = destination });
        }

        public void SetDestination(GameObject destination)
        {
            SetPathfindingRequest(new PathfindingRequest() { destinationTarget = destination });
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

        public void Update()
        {
            if (RequiresFindingNewPath)
            {
                var navWorld = NavWorldReference.Instance.NavWorld;
                var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
                _currentPathFinding = _desiredPathFinding;
                _targetSegment = navWorld.GetClosestNavSegment(_currentPathFinding.Value.DestinationPosition, ConnectionType.Walk, segmentProximityMaxDistance);
                _desiredPathFinding = null;
                _coroutine = _context.StartCoroutine(FindPath(ReferencePosition, _currentPathFinding.Value));
            }

            UpdatePathSegment();
            Move();
        }

        private void SetPathfindingRequest(PathfindingRequest request)
        {
            CancelPathFinding();
            _desiredPathFinding = request;
            if (RequiresFindingNewPath)
            {
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
            _path[_path.Length - 1].position = _currentPathFinding.Value.destinationTarget.transform.position;
        }

        private void Move()
        {
            if (_path == null)
            {
                return;
            }

            if (!TargetInSameSegment())
            {
                _coroutine = _context.StartCoroutine(FindPath(ReferencePosition, _currentPathFinding.Value));
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

        private IEnumerator FindPath(Vector2 start, PathfindingRequest end)
        {
            var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
            var endPos = end.destinationPoint.HasValue ? end.destinationPoint.Value : (Vector2)end.destinationTarget.transform.position;

            IsSearchingForPath = true;
            var taskCoroutine = _context.FindPath(
                start,
                endPos,
                _height,
                _maxSlopeDegrees,
                _allowedConnectionTypes,
                _pathfindingMaxDistance,
                segmentProximityMaxDistance,
                _navTagCapable,
                _stepHeight,
                _connectionMultipliers);

            //_pathfindingTask = Task.Run(() => 
            //    GuideAgentHelper.PathfindingTask(start, end.destinationPoint.Value, _height, _maxSlopeDegrees, _allowedConnectionTypes, _pathfindingMaxDistance, segmentProximityMaxDistance, _navTagCapable, _stepHeight, _connectionMultipliers));

            yield return taskCoroutine;
            IsSearchingForPath = false;

            if (taskCoroutine.Exception != null)
            {
                Debug.LogException(taskCoroutine.Exception);
                throw taskCoroutine.Exception;
            }

            var result = taskCoroutine.Result;

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
                _context.StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _desiredPathFinding = null;
            _currentPathFinding = null;
            _agentStatus = AgentStatus.Iddle;
            _path = null;
            DesiredMovement = Vector2.zero;
        }

        private bool TargetInSameSegment()
        {
            if (!_currentPathFinding.HasValue)
            {
                return false;
            }

            if (_currentPathFinding.Value.destinationTarget == null)
            {
                return false;
            }

            var navWorld = NavWorldReference.Instance.NavWorld;
            var segmentProximityMaxDistance = _settings.SegmentProximityMaxDistance;
            var navSegment = navWorld.GetClosestNavSegment(_currentPathFinding.Value.destinationTarget.transform.position, ConnectionType.Walk, segmentProximityMaxDistance);

            return navSegment.segment.IsCoincident(_targetSegment.Value.segment);
        }
    }
}