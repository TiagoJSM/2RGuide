using System;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    [Serializable]
    public class NavTagSetting
    {
        [SerializeField]
        private string _tag;
        [SerializeField]
        private Color _color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        public string Tag => _tag;
        public Color Color => _color;
    }
}