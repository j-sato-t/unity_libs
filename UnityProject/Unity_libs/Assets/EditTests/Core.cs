using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections;
using UniLib.Core;
using UnityEngine.TestTools;

public class Core
{
    // A Test behaves as an ordinary method
    [TestCase("")]
    [TestCase("TestTag")]
    public void Manageable_Base(string tag)
    {
        var manageable = UniLib.Core.Manageable.Instantiate<UniLib.Core.Manageable>(new UniLib.Core.ManageableSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
            LogNameTag = tag,
        });
        Assert.IsNotNull(manageable);

        Assert.IsTrue(manageable.Open());
        Assert.IsTrue(manageable.Pause());
        Assert.IsTrue(manageable.Resume());
        manageable.Close();
    }

    [UnityTest]
    public IEnumerator Manageable_WaitOpen()
    {
        var wt = WaitTest.Instantiate<WaitTest>(new ManageableSetting
        {
            FilterLevel= UniLib.Log.LogLevel.Trace,
        });

        wt.Open();
        while(!wt.IsRunning) yield return null;
    }

    private class WaitTest : Manageable
    {
        protected override bool OnOpen()
        {
            AddOpeningAct(WaitMethod);
            AddOpeningAct(async () =>
            {
                Logger.LogInfo("start lambda act");
                await UniTask.WaitForSeconds(2);
                Logger.LogInfo("end lambda act");
                return true;
            });
            return base.OnOpen();
        }

        private async UniTask<bool> WaitMethod()
        {
            Logger.LogInfo("start wait act");
            await UniTask.WaitForSeconds(1);
            Logger.LogInfo("end wait act");
            return true;
        }

        protected override void OnReady()
        {
            base.OnReady();
            Logger.LogInfo("on call ready");
        }
    }
}


