using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UniLib.Task
{
    /// <summary>
    /// 複数のUniTaskを実行し、その終了を待つ
    /// </summary>
    public class MultiTaskWaiter
    {
        /// <summary>
        /// 管理可能なタスクを実行するメソッドのデリゲート
        /// </summary>
        /// <returns>タスクが成功したか</returns>
        public delegate UniTask<bool> HandleableTaskDelegate();

        private List<HandleableTaskDelegate> _taskMethodList = new List<HandleableTaskDelegate>();
        public bool HasTask => _taskMethodList.Count > 0;

        // ----------------------------------------

        private int _finishCount = 0;
        private bool _failed = false;

        // ----------------------------------------

        private bool _started = false;

        // =======================================================

        /// <summary>
        /// 実行するタスクを登録する
        /// </summary>
        /// <param name="taskMethod">boolを返すUniTaskメソッド</param>
        /// <returns>登録に成功したか</returns>
        public bool AddTask(HandleableTaskDelegate taskMethod)
        {
            if (taskMethod == null) return false;
            if (_started) return false;

            _taskMethodList.Add(taskMethod);
            return true;
        }

        // ----------------------------------------

        private async UniTask RunTask(HandleableTaskDelegate taskMethod)
        {
            var result = await taskMethod();
            _finishCount++;
            if (!result) _failed = true;
        }

        /// <summary>
        /// タスクの実行と待機を開始する
        /// </summary>
        /// <returns>すべてのタスクがTrueを返したか</returns>
        public async UniTask<bool> StartWaitAsync(UnityAction<bool> resultCB = null)
        {
            if (_started)
            {
                resultCB?.Invoke(false);
                return false;
            }
            _started = true;

            foreach (var task in _taskMethodList)
            {
                RunTask(task).Forget();
            }

            // すべてが終了するのを待つ
            await UniTask.WaitUntil(() => _finishCount >= _taskMethodList.Count);

            resultCB?.Invoke(!_failed);
            return !_failed;
        }

        /// <summary>
        /// タスクの実行と待機を開始する
        /// </summary>
        /// <param name="resultCB">終了時のコールバック</param>
        public void StartWait(UnityAction<bool> resultCB)
        {
            StartWaitAsync(resultCB).Forget();
        }
    }
}