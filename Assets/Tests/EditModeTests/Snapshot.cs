using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Assets.Tests.EditModeTests
{
    [Serializable]
    public struct WorldSnapshop
    {
        public NavSegment[] segments;
        public LineSegment2D[] drops;
        public LineSegment2D[] jumps;
    }
    public static class Snapshot
    {
        private static string SnapshotFileName 
        {
            get
            {
                var filename = TestContext.CurrentContext.Test.FullName;
                return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
            }
        }

        private static string SnapshotPath => Path.Combine(Application.dataPath, "__snapshots__", SnapshotFileName);

        public static void Test(NavWorld world)
        {
            EnsureSnapshotDir();
            
            var expectedSnapshot = GetSnapshotObjectJson(world);
            var currentSnapshot = ActualSnapshotJson();

            if (currentSnapshot != null)
            {
                Assert.AreEqual(expectedSnapshot, currentSnapshot);
            }

            SaveSnapshot(expectedSnapshot);
        }

        private static void EnsureSnapshotDir()
        {
            var snapshotDirPath = Path.Combine(Application.dataPath, "__snapshots__");
            Directory.CreateDirectory(snapshotDirPath);
        }

        private static string GetSnapshotObjectJson(NavWorld world)
        {
            var snapshot = new WorldSnapshop()
            {
                segments = world.Segments,
                drops = world.Drops,
                jumps = world.Jumps,
            };

            return JsonUtility.ToJson(snapshot, true);
        }

        private static string ActualSnapshotJson()
        {
            var snapshotPath = SnapshotPath;
            return File.Exists(snapshotPath) ? File.ReadAllText(snapshotPath) : default;
        }

        private static void SaveSnapshot(string json)
        {
            using (var writetext = new StreamWriter(SnapshotPath))
            {
                writetext.Write(json);
            }
        }
    }
}