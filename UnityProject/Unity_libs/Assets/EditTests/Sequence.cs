using System.Collections;
using Cysharp.Threading.Tasks;
using UniLib.Sequence;
using UniLib.Sequence.Test;
using UnityEngine;
using UnityEngine.TestTools;

public class Sequence
{
    [UnityTest]
    public IEnumerator StubTest()
    {
        bool ended = false;

        var stub = StateStub.Instantiate(new StateStub.StubSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
            TestState = WaitState.Instantiate(new WaitState.StateSetting
            {
                FilterLevel = UniLib.Log.LogLevel.Trace,
                NextState = null,
            }),
            ResultCallback = (res) =>
            {
                Debug.Log($"on end state, result: {res.IsSuccess}, next: {(res.NextState != null ? res.NextState.GetType() : "null")}");
                ended = true;
            },
        });
        stub.Open();

        while (!ended) yield return null;
    }

    private IEnumerator StateMachineTest(State firstState)
    {
        bool ended = false;
        var sm = StateMachine.Instantiate(new StateMachine.MachineSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
            SelfTick = true,
            OnFinishCB = (res) =>
            {
                Debug.Log($"end state machine, result: {res}");
                ended = true;
            },
            FirstState = firstState,
        });

        sm.Open();
        while (!ended) yield return null;
    }

    [UnityTest]
    public IEnumerator StateMachineTest_one()
    {
        yield return StateMachineTest(WaitState.Instantiate(new WaitState.StateSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
            NextState = null,
        }));
    }

    [UnityTest]
    public IEnumerator StateMachineTest_oneNext()
    {
        yield return StateMachineTest(NextWaitState.Instantiate(new NextWaitState.StateSetting
        {
            FilterLevel= UniLib.Log.LogLevel.Trace,
        }));
    }

    [UnityTest]
    public IEnumerator StateMachineTest_onePush()
    {
        yield return StateMachineTest(PushWaitState.Instantiate(new PushWaitState.StateSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
        }));
    }

    [UnityTest]
    public IEnumerator StateMachineTest_NextNext()
    {
        yield return StateMachineTest(WaitState.Instantiate(new WaitState.StateSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
            NextState = NextWaitState.Instantiate(new NextWaitState.StateSetting { FilterLevel = UniLib.Log.LogLevel.Trace, }),
        }));
    }

    [UnityTest]
    public IEnumerator StateMachineTest_PushNext()
    {
        yield return StateMachineTest(WaitState.Instantiate(new WaitState.StateSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
            PushState = NextWaitState.Instantiate(new NextWaitState.StateSetting {  FilterLevel = UniLib.Log.LogLevel.Trace,}),
        }));
    }

    private class WaitState : State
    {
        public new class StateSetting : State.StateSetting
        {
            public State NextState;
            public State PushState;
        }
        protected new StateSetting Setting => base.Setting as StateSetting;

        public static WaitState Instantiate(StateSetting setting)
        {
            return Instantiate<WaitState>(setting);
        }

        protected override bool OnOpen()
        {
            AddOpeningAct(async () =>
            {
                await UniTask.Delay(1000);
                return true;
            });
            return true;
        }

        protected override void OnReady()
        {
            if (Setting.NextState != null)
            {
                NextState(Setting.NextState);
            }
            else if (Setting.PushState != null)
            {
                PushState(Setting.PushState);
            }
            else
            {
                EndState();
            }
        }

        protected override bool OnResume()
        {
            EndState();
            return base.OnResume();
        }
    }

    private class NextWaitState : State
    {
        public new class StateSetting : State.StateSetting
        {
            public bool dummy;
        }
        protected new StateSetting Setting => base.Setting as StateSetting;
        public static NextWaitState Instantiate(StateSetting setting)
        {
            return Instantiate<NextWaitState>(setting);
        }

        protected override bool OnOpen()
        {
            return base.OnOpen();
        }

        protected override void OnReady()
        {
            base.OnReady();

            NextState(WaitState.Instantiate(new WaitState.StateSetting
            {
                FilterLevel = Setting.FilterLevel,
                NextState = null,
            }));
        }
    }

    private class PushWaitState : State
    {
        public new class StateSetting : State.StateSetting
        {
            public bool dummy;
        }

        public static PushWaitState Instantiate(StateSetting setting)
        {
            return Instantiate<PushWaitState>(setting);
        }

        protected override void OnReady()
        {
            base.OnReady();
            PushState(WaitState.Instantiate(new WaitState.StateSetting
            {
                FilterLevel = Setting.FilterLevel,
            }));
        }

        protected override bool OnResume()
        {
            EndState();
            return base.OnResume();
        }
    }
}
