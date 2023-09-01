using Cysharp.Threading.Tasks;
using UniLib.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UniLib.Sequence.Test
{
    /// <summary>
    /// ステートを一つだけテストする時用のスタブ
    /// </summary>
    public class StateStub : Core.Manageable
    {
        public class TestResult
        {
            public bool IsSuccess { get; private set; }
            public State NextState { get; private set; }

            public TestResult(bool success, State next = null)
            {
                IsSuccess = success;
                NextState = next;
            }
        }

        // --------------------------------------

        public class StubSetting : Core.ManageableSetting
        {
            public State TestState;
            public UnityAction<TestResult> ResultCallback;
        }
        protected new StubSetting Setting => base.Setting as StubSetting;

        // ===================================

        public static StateStub Instantiate(StubSetting stubSetting)
        {
            return Instantiate<StateStub>(stubSetting);
        }

        // --------------------------------------

        protected override bool OnOpen()
        {
            if (!Checker.NoNullAll(Setting.TestState, Setting.ResultCallback))
            {
                Logger.LogError("fail open, pls check settings");
                return false;
            }
            var state = Setting.TestState;
            state.SetupCallback(OnNext, OnPush, OnEnd);

            if (state.Open())
            {
                AddOpeningAct(async () =>
                {
                    await UniTask.WaitUntil(() => state.IsEndOpen);
                    return !state.IsFailed;
                });
            }
            else
            {
                Logger.LogError("fail open state");
                return false;
            }
            
            return true;
        }

        protected override void OnReady()
        {
            // 成否問わず終了したら閉じる
            Close();
        }

        protected override void OnClose()
        {
            Setting.TestState?.Close();
        }

        // ---------------------------------------

        private void OnNext(State state)
        {
            Logger.LogInfo($"On next state: {state.GetType()}, Not fail: {!Setting.TestState.IsFailed}");
            CallEndCB(state);
        }

        private void OnPush(State state)
        {
            Logger.LogInfo($"On push state: {state.GetType()}");

            // 実行はせず次フレームで再開
            DelayResume().Forget();
        }

        private async UniTask DelayResume()
        {
            await UniTask.NextFrame();
            Setting.TestState.Resume();
        }

        private void OnEnd()
        {
            Logger.LogInfo($"On end state, Not fail: {!Setting.TestState.IsFailed}");
            CallEndCB();
        }

        private void CallEndCB(State next = null)
        {
            var res = new TestResult(!Setting.TestState.IsFailed, next);
            Setting.ResultCallback(res);
        }
    }
}
