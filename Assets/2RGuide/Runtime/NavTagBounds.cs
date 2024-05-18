using Assets._2RGuide.Runtime.Math;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    public class NavTagBounds : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider2D _collider;
        [SerializeField]
        private NavTag _navTag;

        public BoxCollider2D Collider => _collider;
        public NavTag NavTag => _navTag;

        public bool Contains(LineSegment2D lineSeg)
        {
            var bounds = _collider.bounds;
            return bounds.Contains(lineSeg.P1) && bounds.Contains(lineSeg.P2);
        }
    }
}