using Assets._2RGuide.Runtime.Helpers;
using System;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    [Serializable]
    public class NavTag
    {
        [SerializeField]
        private int _tag = -1;

        public static string TagPath => nameof(_tag);

        public int Tag => _tag;
        public override int GetHashCode() => _tag.GetHashCode();
        public override bool Equals(object obj)
        {
            if(obj is NavTag navTag)
            {
                return _tag == navTag._tag;
            }
            return false;
        }

        public static implicit operator bool(NavTag navSegment)
        {
            return navSegment != null && navSegment._tag >= 0;
        }
    }
}