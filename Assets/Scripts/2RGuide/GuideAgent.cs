using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts._2RGuide
{
    public class GuideAgent : MonoBehaviour
    {
        private Node[] _allNodes;
        private Node[] _path;
        private int _targetPathIndex;
        private int _targetNodeIndex;

        [SerializeField]
        private float _speed;
        [SerializeField]
        private NavWorld _navWorld;

        // Use this for initialization
        void Start()
        {
            _allNodes = _navWorld.nodes;
            var astar = new AStar();

            while (_path == null)
            {
                _targetNodeIndex = UnityEngine.Random.Range(0, _allNodes.Length);
                _path = astar.Resolve(_allNodes[UnityEngine.Random.Range(0, _allNodes.Length)], _allNodes[_targetNodeIndex]);
            }
            transform.position = _path[0].Position;
            _targetPathIndex = 1;
        }

        // Update is called once per frame
        void Update()
        {
            var step = _speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, _path[_targetPathIndex].Position, step);

            //if the values are not approximate 
            if (Approximatelly(transform.position, _path[_targetPathIndex].Position))
            {
                _targetPathIndex++;
                if(_targetPathIndex >= _path.Length)
                {
                    _targetPathIndex = 1;

                    var astar = new AStar();
                    var currentNodeIndex = _targetNodeIndex;
                    _targetNodeIndex = UnityEngine.Random.Range(0, _allNodes.Length);
                    _path = astar.Resolve(_allNodes[currentNodeIndex], _allNodes[_targetNodeIndex]);
                }
            }
        }

        private bool Approximatelly(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }
    }
}