using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    [RequireComponent(typeof(GuideAgent))]
    public class TransformMovement : MonoBehaviour
    {
        private GuideAgent _guideAgent;

        public GuideAgent GuideAgent => _guideAgent;

        [SerializeField]
        private Transform _target;

        public Transform Target
        {
            get => _target;
            set
            {
                if (_target != value)
                {
                    _target = value;
                    _guideAgent.SetDestination(_target.position);
                }
            }
        }

        // Use this for initialization
        void Awake()
        {
            _guideAgent = GetComponent<GuideAgent>();
        }

        private void Start()
        {
            if (_target != null)
            {
                _guideAgent.SetDestination(_target.position);
            }
        }

        // Update is called once per frame
        void Update()
        {
            transform.position += (Vector3)_guideAgent.DesiredMovement;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (_guideAgent != null && _target != null)
                {
                    _guideAgent.SetDestination(_target.position);
                }
            }
        }
    }
}