using _2RGuide.Helpers;
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
            if(RequiresFindingNewPath)
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
                    pathStatus = nodes.Last().Position == end ? PathStatus.Complete : PathStatus.Incomplete;
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

            var agentSegmentPath =
                path
                    .Select((n, i) =>
                    {
                        var connectionType =
                            i == 0
                            ? ConnectionType.Walk
                            : path[i - 1].ConnectionWith(path[i]).Value.ConnectionType;
                        return new AgentSegment() { position = n.Position, connectionType = connectionType };
                    });

            var segmentPath = agentSegmentPath.ToArray();

            // if character is already in between first and second node no need to go back to first
            if (path.Count() > 1)
            {
                var closestPositionWithStart = path[0].ConnectionWith(path[1]).Value.Segment.ClosestPointOnLine(ReferencePosition);
                segmentPath[0].position = closestPositionWithStart;
            }

            _coroutine = null;

            // if character doesn't want to move to last node it should stay "half way"
            if (path.Count() > 1)
            { 
                //ToDo: first check if on segment between length-2 and length-1, if yes run code bellow, otherwise check connections for last node for closest value on segment
                var closestPositionWithTarget = path[path.Length - 2].ConnectionWith(path.Last()).Value.Segment.ClosestPointOnLine(_currentDestination.Value);
                segmentPath[segmentPath.Length - 1].position = closestPositionWithTarget;
            }

            _path = segmentPath;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ReferencePosition, ProximityThreshold);
        }
    }
}