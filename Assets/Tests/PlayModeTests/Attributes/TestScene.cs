using NUnit.Framework.Interfaces;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Tests.PlayModeTests.Attributes
{
    public class TestScene : NUnitAttribute, ITestAction
    {
        private string _scene;

        public ActionTargets Targets { get { return ActionTargets.Test; } }

        public TestScene(string scene)
        {
            _scene = scene;
        }

        public void AfterTest(ITest test)
        {

        }

        public void BeforeTest(ITest test)
        {
            TestSceneManager.LoadScene(_scene);
        }
    }
}