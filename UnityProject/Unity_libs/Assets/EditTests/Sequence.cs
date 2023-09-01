using System.Collections;
using Cysharp.Threading.Tasks;
using UniLib.Sequence;
using UniLib.Sequence.Test;
using UnityEngine;
using UnityEngine.TestTools;

public class Sequence
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator StubTest()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
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

    private class WaitState : State
    {
        public new class StateSetting : State.StateSetting
        {
            public State NextState;
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
            else
            {
                EndState();
            }
        }
    }
}
