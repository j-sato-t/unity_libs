using UnityEngine;
using UnityEngine.Events;

namespace UniLib.Sequence
{
    public class State : Core.Manageable
    {
        public class StateSetting : Core.ManageableSetting
        {
        }
        protected new StateSetting Setting => base.Setting as StateSetting;

        // ----------------------------------

        private UnityEvent<State> _onNextState = new UnityEvent<State>();
        private UnityEvent<State> _onPushState = new UnityEvent<State>();
        private UnityEvent _onEndState = new UnityEvent();

        // ===========================================================
        public static State Instantiate(StateSetting setting)
        {
            return Instantiate<State>(setting);
        }
        // ===========================================================

        protected void NextState(State state)
        {
            if (!IsRunning) return;

            if (state == null)
            {
                Logger.LogError("Fail NextState, set state is null");
                return;
            }

            Logger.LogTrace($"Call NextState: {state.GetType()}");
            _onNextState.Invoke(state);
        }

        protected void PushState(State state)
        {
            if (!IsRunning) return;

            if (state == null)
            {
                Logger.LogError("Fail PushState, set state is null");
                return;
            }

            Logger.LogTrace($"Call PushState: {state.GetType()}");
            _onPushState.Invoke(state);
        }

        protected void EndState()
        {
            if (!IsRunning) return;

            Logger.LogTrace("Call EndState");
            _onEndState.Invoke();
        }

        public bool SetupCallback(UnityAction<State> onNext, UnityAction<State> onPush, UnityAction onEnd)
        {
            if (!Core.Checker.NoNullAll(onNext, onPush, onEnd)) return false;

            _onNextState.AddListener(onNext);
            _onPushState.AddListener(onPush);
            _onEndState.AddListener(onEnd);
            return true;
        }
    }
}
