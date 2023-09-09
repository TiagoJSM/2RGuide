using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts._2RGuide
{
    [InitializeOnLoad]
    public class NavWorldReference : Singleton<NavWorldReference>
    {
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