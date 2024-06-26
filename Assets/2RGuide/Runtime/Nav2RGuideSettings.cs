﻿using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    // Create a new type of Settings Asset.
    public class Nav2RGuideSettings : ScriptableObject
    {
        private const string ResourcesFolder = "Settings";
        private const string FileName = "Nav2RGuideSettings";
        private static readonly string ResourceLoadPath = $"{ResourcesFolder}/{FileName}";

        public static readonly string SettingsPath = $"Assets/Resources/{ResourcesFolder}";
        public static readonly string Nav2RGuideSettingsPath = $"{SettingsPath}/{FileName}.asset";

        [SerializeField]
        private float _maxDropHeight = 10.0f;
        [SerializeField]
        private float _jumpDropHorizontalDistance = 1.0f;
        [Range(0.0f, 90.0f)]
        [SerializeField]
        private float _maxSlope = 60.0f;
        [SerializeField]
        private float _maxJumpHeight;
        [SerializeField]
        private float _segmentDivisionDistance = 1.0f;
        [SerializeField]
        private LayerMask _oneWayPlatformMask;
        [SerializeField]
        private float _segmentMaxHeight = 50.0f;
        [SerializeField]
        private float _segmentProximityMaxDistance = 0.0f;
        [SerializeField]
        [NonReorderable]
        private NavTagSetting[] _navTags;
        [SerializeField]
        private NavTag[] _noDropsOrJumpsTargetTags;
        [Header("Debug")]
        [SerializeField]
        private float _agentTargetPositionDebugSphereRadius;
        [SerializeField]
        private float _agentDebugPathVerticalOffset;

        public float MaxDropHeight => _maxDropHeight;
        public float JumpDropHorizontalDistance => _jumpDropHorizontalDistance;
        public float MaxSlope => _maxSlope;
        public float MaxJumpHeight => _maxJumpHeight;
        public float SegmentDivisionDistance => _segmentDivisionDistance;
        public LayerMask OneWayPlatformMask => _oneWayPlatformMask;
        public float SegmentMaxHeight => _segmentMaxHeight;
        public float SegmentProximityMaxDistance => _segmentProximityMaxDistance;
        public NavTagSetting[] NavTagsSettings => _navTags;
        public NavTag[] NoDropsOrJumpsTargetTags => _noDropsOrJumpsTargetTags;
        public float AgentTargetPositionDebugSphereRadius => _agentTargetPositionDebugSphereRadius;
        public float AgentDebugPathVerticalOffset => _agentDebugPathVerticalOffset;

        public static string[] SettingFields => new string[]
            {
                nameof(_maxDropHeight),
                nameof(_jumpDropHorizontalDistance),
                nameof(_maxSlope),
                nameof(_maxJumpHeight),
                nameof(_segmentDivisionDistance),
                nameof(_oneWayPlatformMask),
                nameof(_segmentMaxHeight),
                nameof(_segmentProximityMaxDistance),
                nameof(_navTags),
                nameof(_noDropsOrJumpsTargetTags),
                nameof(_agentTargetPositionDebugSphereRadius),
                nameof(_agentDebugPathVerticalOffset),
            };

        public static Nav2RGuideSettings Load()
        {
            return Resources.Load<Nav2RGuideSettings>(ResourceLoadPath);
        }

        public NavTagSetting GetSettingForNavTag(NavTag navTag)
        {
            if(navTag)
            {
                return _navTags[navTag.Tag];
            }
            return null;
        }
    }
}