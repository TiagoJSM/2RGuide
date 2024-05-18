using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._2RGuide.Runtime
{
    public class NavWorldReference : Singleton<NavWorldReference>
    {
        [RuntimeInitializeOnLoadMethod]
        private static void OnRuntimeMethodLoad()
        {
            var tmp = Instance;
        }

        private NavWorld _navWorld;

        public NavWorld NavWorld => _navWorld;

        private void Awake()
        {
            FindNavworld();
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            FindNavworld();
        }

        private void FindNavworld()
        {
            _navWorld = UnityEngine.Object.FindObjectOfType<NavWorld>();
        }
    }
}