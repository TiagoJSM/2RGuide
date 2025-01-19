using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    public enum MoveTo
    {
        Position,
        Target,
    }

    [RequireComponent(typeof(GuideAgent))]
    public class TransformMovement : MonoBehaviour
    {
        private GuideAgent _guideAgent;

        public GuideAgent GuideAgent => _guideAgent;

        [SerializeField]
        private Transform _target;
        [SerializeField]
        private MoveTo _moveTo;
        [SerializeField]
        private bool _allowIncompletePath = true;

        public Transform Target
        {
            get => _target;
            set
            {
                if (_target != value)
                {
                    _target = value;
                    SetDestination();
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
                SetDestination();
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
                    _guideAgent.SetDestination(_target.position, _allowIncompletePath, 0.0f);
                }
            }
        }

        private void SetDestination()
        {
            if (_moveTo == MoveTo.Position)
            {
                _guideAgent.SetDestination(_target.position, _allowIncompletePath, 0.0f);
            }
            else
            {
                _guideAgent.SetDestination(_target.gameObject, _allowIncompletePath, 0.0f);
            }
        }
    }
}