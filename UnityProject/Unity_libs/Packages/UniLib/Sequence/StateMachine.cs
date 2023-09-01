using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UniLib.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UniLib.Sequence
{
    public class StateMachine : Manageable
    {
        public class MachineSetting : ManageableSetting
        {
            public State FirstState;
            public UnityAction<bool> OnFinishCB;
        }
        protected new MachineSetting Setting => base.Setting as MachineSetting;

        // -------------------------------------------------
        private Stack<State> _stateStack;

        // =======================================================

        public static StateMachine Instantiate(MachineSetting setting)
        {
            return Instantiate<StateMachine>(setting);
        }

        // =======================================================

        protected override bool OnOpen()
        {
            if (!Core.Checker.NoNullAll(Setting.FirstState))
            {
                Logger.LogError("fail open, pls check Setting.FirstState");
                return false;
            }

            if (!Setting.SelfTick)
            {
                Logger.LogWarning("no set Setting.SelfTick, Update is manual?");
            }

            _stateStack = new Stack<State>();

            DelayStart().Forget();

            return true;
        }

        private async UniTask DelayStart()
        {
            await UniTask.WaitUntil(() => IsRunning);
            StartState(Setting.FirstState);
        }

        protected override void OnUpdate(float deltaMs)
        {
            base.OnUpdate(deltaMs);

            State peek;
            if (_stateStack.TryPeek(out peek))
            {
                peek.Update(deltaMs);
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            Setting.OnFinishCB?.Invoke(!IsFailed);
        }

        // -------------------------------------------

        private void StartState(State state)
        {
            state.SetupCallback(OnNextState, OnPushState, OnEndState);
            _stateStack.Push(state);
            Logger.LogTrace($"open state: {state.GetType()}");
            if (!state.Open())
            {
                // 失敗時はそのステート処理を中止
                OnEndState();
            }
        }

        private void OnNextState(State nextState)
        {
            if (!IsRunning) return;

            Logger.LogTrace("OnNextState");
            if (nextState == null)
            {
                Logger.LogError("next state is null");
                SetFailed();
                Close();
                return;
            }

            // 現在のステートを終了
            State pop;
            if (_stateStack.TryPop(out pop))
            {
                Logger.LogTrace($"pop state: {pop.GetType()}");
                pop.Close();
            }

            // 次のステートを開始して追加
            StartState(nextState);
        }

        private void OnPushState(State childState)
        {
            if (!IsRunning) return;

            Logger.LogTrace("OnPushState");

            if (childState == null)
            {
                Logger.LogError("push state is null");
                SetFailed();
                Close();
                return;
            }

            // 現在のステートを中断
            State peek;
            if (_stateStack.TryPeek(out peek))
            {
                Logger.LogTrace($"pause state: {peek.GetType()}");
                peek.Pause();
            }

            // 次のステートを開始して追加
            StartState(childState);
        }

        private void OnEndState()
        {
            if (!IsRunning) return;

            Logger.LogTrace("OnEndState");

            // 現在のステートを終了
            var pop = _stateStack.Pop();
            Logger.LogTrace($"close state: {pop.GetType()}");
            pop.Close();

            // 待機していたステートの再開
            State peek;
            if (_stateStack.TryPeek(out peek))
            {
                Logger.LogTrace($"resume state: {peek.GetType()}");
                if (!peek.Resume())
                {
                    Logger.LogError($"fail resume state: {peek.GetType()}");
                    SetFailed();
                    OnEndState();
                }
            }
            else
            {
                // 待機ステートが無い = すべて終了
                Close();
            }
        }
    }
}
