using Assets._2RGuide.Runtime.Helpers;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    [Serializable]
    public struct RGuideVector2
    {
        [SerializeField]
        private Vector2 _vec;

        public float x => _vec.x;
        public float y => _vec.y;
        public RGuideVector2 normalized => new RGuideVector2(_vec.normalized);
        public float sqrMagnitude => _vec.sqrMagnitude;

        public static RGuideVector2 zero => new RGuideVector2(Vector2.zero);
        public static RGuideVector2 one => new RGuideVector2(Vector2.one);
        public static RGuideVector2 down => new RGuideVector2(Vector2.down);

        public RGuideVector2(float value)
            :this(value, value)
        {
        }

        public RGuideVector2(float x, float y)
        {
            _vec = new Vector2(x.Round(Constants.RoundingDecimalPrecision), y.Round(Constants.RoundingDecimalPrecision));
        }

        public RGuideVector2(Vector2 vec)
            :this(vec.x, vec.y)
        {
        }

        public RGuideVector2(Vector3 vec)
            : this((Vector2)vec)
        {
        }

        public bool Approximately(RGuideVector2 other)
        {
            return x.Approximately(other.x) && y.Approximately(other.y);
        }

        public Vector2 ToVector2() => _vec;

        public string ToString(string format)
        {
            return _vec.ToString(format);
        }

        public override bool Equals(object obj)
        {
            return obj is RGuideVector2 other && _vec.Equals(other._vec);
        }

        public override int GetHashCode()
        {
            return _vec.GetHashCode();
        }

        public static RGuideVector2 MoveTowards(RGuideVector2 current, RGuideVector2 target, float maxDistanceDelta)
        {
            return new RGuideVector2(Vector2.MoveTowards(current._vec, target._vec, maxDistanceDelta));
        }

        public static float Distance(RGuideVector2 p1, RGuideVector2 p2)
        {
            return Vector2.Distance(p1._vec, p2._vec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(RGuideVector2 lhs, RGuideVector2 rhs)
        {
            return Vector2.Dot(lhs._vec, rhs._vec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGuideVector2 operator -(RGuideVector2 a, RGuideVector2 b)
        {
            return new RGuideVector2(a._vec - b._vec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGuideVector2 operator +(RGuideVector2 a, RGuideVector2 b)
        {
            return new RGuideVector2(a._vec + b._vec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGuideVector2 operator /(RGuideVector2 a, float d)
        {
            return new RGuideVector2(a.x / d, a.y / d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGuideVector2 operator *(RGuideVector2 a, float d)
        {
            return new RGuideVector2(a._vec * d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RGuideVector2 lhs, RGuideVector2 rhs)
        {
            return lhs._vec == rhs._vec;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RGuideVector2 lhs, RGuideVector2 rhs)
        {
            return !(lhs == rhs);
        }
    }
}
