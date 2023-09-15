﻿using Hextant;
using Hextant.Editor;
using UnityEditor;
using UnityEngine;

namespace _2RGuide.Editor
{
    [Settings(SettingsUsage.EditorProject, "Nav 2RGuide Settings")]
    public sealed class Nav2RGuideSettings : Settings<Nav2RGuideSettings>
    {
        [SettingsProvider]
        static SettingsProvider GetSettingsProvider() => instance.GetSettingsProvider();

        [SerializeField]
        private float _maxDropHeight = 10.0f;
        [SerializeField]
        private float _horizontalDistance = 1.0f;
        [Range(0.0f, 90.0f)]
        [SerializeField]
        private float _maxSlope = 60.0f;
        [SerializeField]
        private float _maxJumpDistance;
        [SerializeField]
        private float _segmentDivision = 1.0f;
        [SerializeField]
        private LayerMask _oneWayPlatformMask;

        public float MaxDropHeight => _maxDropHeight;
        public float HorizontalDistance => _horizontalDistance;
        public float MaxSlope => _maxSlope;
        public float MaxJumpDistance => _maxJumpDistance;
        public float SegmentDivision => _segmentDivision;
        public LayerMask OneWayPlatformMask => _oneWayPlatformMask;
    }
}