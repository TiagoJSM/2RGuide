using Assets.Scripts._2RGuide.Helpers;
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

        private AgentSegment[] _path;
        private int _targetPathIndex;

        private Vector2? _currentDestination;
        private Vector2? _desiredDestination;
        private AgentStatus _agentStatus = AgentStatus.Iddle;
        private Coroutine _coroutine;

        [SerializeField]
        private float _speed;

        public Vector2 DesiredMovement { get; private set; }
        public ConnectionType CurrentConnectionType { get; private set; }

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
        }

        // Update is called once per frame
        void Update()
        {
            if (!_currentDestination.HasValue && _desiredDestination.HasValue)
            {
                CancelPathFinding();
                _currentDestination = _desiredDestination;
                _desiredDestination = null;
                _coroutine = StartCoroutine(FindPath(transform.position, _currentDestination.Value));
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

            if (Approximatelly(transform.position, _path[_targetPathIndex].position))
            {
                _targetPathIndex++;
                if (_targetPathIndex >= _path.Length)
                {
                    _desiredDestination = null;
                    _agentStatus = AgentStatus.Iddle;
                    _path = null;
                    DesiredMovement = Vector2.zero;
                    return;
                }
                else
                {
                    CurrentConnectionType = _path[_targetPathIndex].connectionType;
                }
            }

            if (_targetPathIndex < _path.Length)
            {
                DesiredMovement = Vector2.MoveTowards(transform.position, _path[_targetPathIndex].position, step) - (Vector2)transform.position;
            }
        }

        private bool Approximatelly(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }

        private IEnumerator FindPath(Vector2 start, Vector2 end)
        {
            _agentStatus = AgentStatus.Busy;
            var pathfindingTask = Task.Run(() => 
            {
                var navWorld = NavWorldReference.Instance.NavWorld;
                var allNodes = navWorld.nodes;
                var startN = allNodes.MinBy(n => Vector2.Distance(start, n.Position));
                var endN = allNodes.MinBy(n => Vector2.Distance(end, n.Position));
                return AStar.Resolve(startN, endN);
            });

            while (!pathfindingTask.IsCompleted)
            {
                yield return null;
            }

            var path = pathfindingTask.Result;
            _targetPathIndex = 0;

            var agentSegmentPath =
                path
                    .Select((n, i) =>
                    {
                        var connectionType =
                            i == 0
                            ? ConnectionType.Walk
                            : path[i - 1].ConnectionWith(path[i]).Value.connectionType;
                        return new AgentSegment() { position = n.Position, connectionType = connectionType };
                    });

            var segmentPath = agentSegmentPath.ToArray();

            // if character is already in between first and second node no need to go back to first
            if (path.Count() > 1)
            {
                var closestPositionWithStart = path[0].ConnectionWith(path[1]).Value.segment.ClosestPointOnLine(transform.position);
                segmentPath[0].position = closestPositionWithStart;
            }

            _coroutine = null;

            // if character doesn't want to move to last node it should stay "half way"
            if (path.Count() > 1)
            { 
                //ToDo: first check if on segment between length-2 and length-1, if yes run code bellow, otherwise check connections for last node for closest value on segment
                var closestPositionWithTarget = path[path.Length - 2].ConnectionWith(path.Last()).Value.segment.ClosestPointOnLine(_currentDestination.Value);
                segmentPath[segmentPath.Length - 1].position = closestPositionWithTarget;
            }

            _path = segmentPath;
        }
    }
}