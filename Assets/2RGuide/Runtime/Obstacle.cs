using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    public class Obstacle : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider2D _collider;

        public BoxCollider2D Collider => _collider;
    }
}