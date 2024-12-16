using Assets._2RGuide.Runtime.Coroutines;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._2RGuide.Runtime
{
    public class NavWorldManager : Singleton<NavWorldManager>
    {
        private Dictionary<GameObject, TaskCoroutine<PathfindingTask.PathfindingResult>> _pathfindingTasks = 
            new Dictionary<GameObject, TaskCoroutine<PathfindingTask.PathfindingResult>>();

        [RuntimeInitializeOnLoadMethod]
        private static void OnRuntimeMethodLoad()
        {
            var tmp = Instance;
        }

        private NavWorld _navWorld;

        public NavWorld NavWorld => _navWorld;
        public IEnumerable<GameObject> Callers => _pathfindingTasks.Keys;

        public TaskCoroutine<PathfindingTask.PathfindingResult> RunPathfinding(
            GameObject caller,
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
            StopPathfindingFor(caller);
            var task = TaskCoroutine<PathfindingTask.PathfindingResult>
                .Run(() =>
                    PathfindingTask.Run(
                        _navWorld,
                        start,
                        end,
                        maxHeight,
                        maxSlopeDegrees,
                        allowedConnectionTypes,
                        pathfindingMaxDistance,
                        segmentProximityMaxDistance,
                        navTagCapable,
                        stepHeight,
                        connectionMultipliers));

            _pathfindingTasks.Add(caller, task);

            return task
                .ContinueWith((t) => 
                    StopPathfindingFor(caller));
        }

        private void Awake()
        {
            FindNavworld();
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            FindNavworld();
        }

        private void FindNavworld()
        {
            _navWorld = UnityEngine.Object.FindObjectOfType<NavWorld>();
        }

        private void StopPathfindingFor(GameObject caller)
        {
            if(_pathfindingTasks.TryGetValue(caller, out var task))
            {
                _pathfindingTasks.Remove(caller);
            }
        }
    }
}