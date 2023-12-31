﻿using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class LayerMaskExtensions
    {
        public static bool Includes(this LayerMask mask, GameObject go)
        {
            return (mask.value & 1 << go.layer) > 0;
        }
    }
}