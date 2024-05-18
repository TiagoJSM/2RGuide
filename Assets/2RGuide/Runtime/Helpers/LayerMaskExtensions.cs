using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class LayerMaskExtensions
    {
        public static bool Includes(this LayerMask mask, GameObject go)
        {
            return (mask.value & 1 << go.layer) > 0;
        }
    }
}