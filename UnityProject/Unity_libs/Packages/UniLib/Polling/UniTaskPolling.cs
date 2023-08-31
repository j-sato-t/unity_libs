using Cysharp.Threading.Tasks;
using System;
using UniLib.Core;
using UnityEngine;

namespace UniLib.Polling
{
    public class UniTaskPolling : Manageable
    {
        public class UniTaskPollingSetting : ManageableSetting
        {
            /// <summary>
            /// 待機するフレームの間隔（1 - 60）
            /// </summary>
            public int FrameInterval = 1;

            public delegate bool PollingActionDelegate(float deltaMs);
            /// <summary>
            /// 周期処理の本体<br>引数はfloatミリ秒<br>戻り値は継続ならtrue、終了ならfalse
            /// </summary>
            public PollingActionDelegate PollingAction;
        }
        protected new UniTaskPollingSetting Setting => base.Setting as UniTaskPollingSetting;

        // -------------------------------------------


        // ================================================================

        public static UniTaskPolling Instantiate(UniTaskPollingSetting setting)
        {
            return Instantiate<UniTaskPolling> (setting);
        }

        // ================================================================

        protected override bool OnOpen()
        {
            if (Setting.PollingAction == null)
            {
                Logger.LogError("pls set Setting.Action");
                return false;
            }

            Setting.FrameInterval = Math.Clamp(Setting.FrameInterval, 1, 60);
            TickLoop().Forget();

            return true;
        }

        // ----------------------------------------------------

        private async UniTask TickLoop()
        {
            Logger.LogDebug($"Start tick, FrameInterval: {Setting.FrameInterval}");
            await UniTask.WaitUntil(() => NowCondition == Condition.Running);

            var lastTime = DateTime.Now;
            while (true)
            {
                await UniTask.DelayFrame(Setting.FrameInterval);
                if (NowCondition != Condition.Running) continue;
                var after = DateTime.Now;
                var delta = after - lastTime;
                if (!Setting.PollingAction.Invoke((float)delta.TotalMilliseconds))
                {
                    break;
                }
                lastTime = after;
            }

            Close();
        }
    }
}
