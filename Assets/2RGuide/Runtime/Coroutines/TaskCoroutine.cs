using Assets._2RGuide.Runtime.Helpers;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Coroutines
{
    public class TaskCoroutine<TResult> : CustomYieldInstruction
    {
        private Task<TResult> _task;
        public override bool keepWaiting => !_task.IsCompleted;
        public TResult Result => _task.Result;
        public Exception Exception => _task.Exception;

        private TaskCoroutine(Task<TResult> task)
        {
            _task = task;
        }

        public static TaskCoroutine<TResult> Run(Func<TResult> action)
        {
            return new TaskCoroutine<TResult>(Task.Run(action));
        }
    }
}