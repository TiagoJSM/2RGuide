using Assets.Scripts._2RGuide.Helpers;
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

        // Update is called once per frame
        void Update()
        {
            Initialize();

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

                    Debug.Log($"{_allNodes[currentNodeIndex].Position}            {_allNodes[_targetNodeIndex].Position}");
                }
            }
        }

        private void Initialize()
        {
            if(_allNodes != null)
            {
                return;
            }

            _allNodes = NavWorldReference.Instance.NavWorld.nodes;

            var astar = new AStar();

            while (_path == null)
            {
                _targetNodeIndex = UnityEngine.Random.Range(0, _allNodes.Length);
                var startNodeIndex = UnityEngine.Random.Range(0, _allNodes.Length);

                var startN = _allNodes[startNodeIndex];
                var endN = _allNodes[_targetNodeIndex];

                //(-1.0, 2.8)            (-1.0, 2.8)
                //startN = _allNodes.FirstOrDefault(n => n.Position.Approximately(new Vector2(0.0f, 3.5f)));
                //endN = _allNodes.FirstOrDefault(n => n.Position.Approximately(new Vector2(-4.0f, 2.25f)));

                _path = astar.Resolve(startN, endN);
                if(_path == null)
                {
                    return;
                }
            }

            transform.position = _path[0].Position;
            _targetPathIndex = 1;
        }

        private bool Approximatelly(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }
    }
}