using Clipper2Lib;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class ClipperUtils
    {
        public static ClipperD ConfiguredClipperD()
        {
            return new ClipperD(3);
        }
    }
}