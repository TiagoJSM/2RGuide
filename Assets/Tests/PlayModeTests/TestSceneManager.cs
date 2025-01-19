using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Tests.PlayModeTests
{
    public static class TestSceneManager
    {
        public static void LoadScene(string sceneName)
        {
            Debug.Log($"Test running on scene {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
}
