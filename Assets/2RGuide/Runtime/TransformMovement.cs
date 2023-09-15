using UnityEngine;

namespace _2RGuide
{
    [RequireComponent(typeof(GuideAgent))]
    public class TransformMovement : MonoBehaviour
    {
        private GuideAgent _guideAgent;

        // Use this for initialization
        void Awake()
        {
            _guideAgent = GetComponent<GuideAgent>();
        }

        private void Start()
        {
            _guideAgent.SetDestination(new Vector2(-4.0f, 2.25f));
        }

        // Update is called once per frame
        void Update()
        {
            transform.position += (Vector3)_guideAgent.DesiredMovement;
        }
    }
}