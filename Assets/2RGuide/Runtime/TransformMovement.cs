using UnityEngine;

namespace _2RGuide
{
    [RequireComponent(typeof(GuideAgent))]
    public class TransformMovement : MonoBehaviour
    {
        private GuideAgent _guideAgent;

        [SerializeField]
        private Transform _target;

        // Use this for initialization
        void Awake()
        {
            _guideAgent = GetComponent<GuideAgent>();
        }

        private void Start()
        {
            _guideAgent.SetDestination(_target.position);
        }

        // Update is called once per frame
        void Update()
        {
            transform.position += (Vector3)_guideAgent.DesiredMovement;
        }
    }
}