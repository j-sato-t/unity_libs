using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniLib.Core
{
    /// <summary>
    /// Manageable の設定基底クラス
    /// </summary>
    public class ManageableSetting
    {
        /// <summary>
        /// ログのデフォルトのフィルタ
        /// </summary>
        public Log.LogLevel FilterLevel = Log.LogLevel.Info;
        /// <summary>
        /// ログにつけるタグ（空だとクラス名になる）
        /// </summary>
        public string LogNameTag = "";

        /// <summary>
        /// 独自にUniTaskでUpdateを呼ぶか
        /// </summary>
        public bool SelfTick = false;
    }


    public class Manageable : ITickable
    {
        internal enum Condition
        {
            /// <summary>
            /// 生成された状態
            /// </summary>
            Created,
            /// <summary>
            /// 開始処理実行中
            /// </summary>
            Opening,
            /// <summary>
            /// 動作中
            /// </summary>
            Running,
            /// <summary>
            /// 中断している
            /// </summary>
            Pause,
            /// <summary>
            /// 終了済
            /// </summary>
            Finished,

            /// <summary>
            /// 失敗時
            /// </summary>
            Failed,
        }
        private Condition _condition;

        internal Condition NowCondition { get => _condition; }
        public bool IsRunning { get => _condition == Condition.Running; }
        public bool IsFailed { get => _condition == Condition.Failed; }

        public bool IsEndOpen { get => _condition >= Condition.Running; }

        /// <summary>
        /// このインスタンスで使うロガー
        /// </summary>
        protected Log.Logger Logger { get; private set; }

        private ManageableSetting _setting;
        /// <summary>
        /// 設定へのアクセス<br>継承した設定クラスはこれをキャストする
        /// </summary>
        protected ManageableSetting Setting => _setting;

        private Task.MultiTaskWaiter _openingActor;


        /// <summary>
        /// 自身のClose時にCloseを呼ぶリスト
        /// </summary>
        private List<Manageable> _autoCloser = new List<Manageable>();

        // ==================================
        /// <summary>
        /// インスタンスの生成
        /// </summary>
        /// <typeparam name="T">Manageable クラス</typeparam>
        /// <param name="setting">設定クラス</param>
        /// <returns>生成したインスタンス</returns>
        public static T Instantiate<T>(ManageableSetting setting) where T : Manageable, new()
        {
            string nameTag = (string.IsNullOrEmpty(setting.LogNameTag)) ? typeof(T).Name : setting.LogNameTag;

            T instance = new()
            {
                _setting = setting,
                Logger = new Log.Logger(setting.FilterLevel, nameTag),
            };
            instance.SetCondition(Condition.Created);

            return instance;
        }

        // ==================================
        /// <summary>
        /// 処理を開始する
        /// </summary>
        /// <returns>開始に成功したか</returns>
        public bool Open()
        {
            Logger.LogTrace("Open");

            if (_condition != Condition.Created)
            {
                Logger.LogError("call Open, but was opened");
                return false;
            }
            SetCondition(Condition.Opening);

            _openingActor = new Task.MultiTaskWaiter();

            if (!OnOpen())
            {
                // 失敗が確定した場合
                SetFailed();

                return false;
            }

            // 自己Tick開始
            if (Setting.SelfTick)
            {
                SelfTick().Forget();
            }

            // 開始処理の待機
            if (_openingActor.HasTask)
            {
                _openingActor.StartWait(result =>
                {
                    if (result)
                    {
                        SetCondition(Condition.Running);
                        OnReady();
                    }
                    else
                    {
                        SetFailed();
                    }
                });
                return true;
            }

            // 待機がなければ即座に完了
            SetCondition(Condition.Running);
            OnReady();
            return true;
        }

        /// <summary>
        /// 処理を一時停止する
        /// </summary>
        /// <returns>成功したか</returns>
        public bool Pause()
        {
            Logger.LogTrace("Pause");

            if (_condition != Condition.Running)
            {
                Logger.LogError("call Pause, but not running");
                return false;
            }
            SetCondition(Condition.Pause);

            return OnPause();
        }

        /// <summary>
        /// 一時停止していた処理を再開する
        /// </summary>
        /// <returns>成功したか</returns>
        public bool Resume()
        {
            Logger.LogTrace("Resume");

            if (_condition != Condition.Pause)
            {
                Logger.LogError("call Resume, but not pause");
                return false;
            }

            SetCondition(Condition.Running);
            if (OnResume())
            {
                return true;
            }
            else
            {
                SetFailed();
                Logger.LogError("fail Resume");
                return false;
            }
        }

        /// <summary>
        /// 処理を終了する
        /// </summary>
        public void Close()
        {
            Logger.LogTrace("Close");

            if (_condition == Condition.Created || _condition == Condition.Finished)
            {
                Logger.LogError("call Close, but not running");
                return;
            }

            SetCondition(Condition.Finished);

            OnClose();

            // 自動終了
            foreach (var manageable in _autoCloser)
            {
                manageable?.Close();
            }
        }

        // ----------------------------
        /// <summary>
        /// 開始処理
        /// </summary>
        /// <returns>成功したか</returns>
        protected virtual bool OnOpen() { return true; }

        /// <summary>
        /// 開始処理が完了した際に呼ばれる
        /// </summary>
        protected virtual void OnReady() { }

        /// <summary>
        /// 中断処理
        /// </summary>
        /// <returns>成功したか</returns>
        protected virtual bool OnPause() { return true; }

        /// <summary>
        /// 再開処理
        /// </summary>
        /// <returns>成功したか</returns>
        protected virtual bool OnResume() { return true; }

        /// <summary>
        /// 終了処理
        /// </summary>
        protected virtual void OnClose() { }

        protected virtual void OnUpdate(float deltaMs) { }

        /// <summary>
        /// 周期処理の呼び出し（外部からでも呼べる）
        /// </summary>
        /// <param name="deltaMs">フレーム経過時間（ミリ秒を想定している）</param>
        public void Update(float deltaMs) 
        {
            if (!IsRunning) return;
            OnUpdate(deltaMs);
        }

        private async UniTask SelfTick()
        {
            await UniTask.WaitUntil(() => NowCondition == Condition.Running);

            DateTime befor, after;
            TimeSpan delta;
            while (true)
            {
                befor = DateTime.Now;
                await UniTask.NextFrame();
                if (NowCondition != Condition.Running) break;
                after = DateTime.Now;
                delta = after - befor;
                Update((float)delta.TotalMilliseconds);
            }
        }

        // ----------------------------

        private void SetCondition(Condition condition)
        {
            _condition = condition;
            Logger.LogTrace($"Set Condition: {_condition}");
        }

        protected void SetFailed()
        {
            SetCondition(Condition.Failed);
        }

        // ----------------------------

        /// <summary>
        /// 待機が必要な開始処理を登録する
        /// </summary>
        /// <param name="handleableTask">実行したいタスクのメソッド</param>
        protected void AddOpeningAct(Task.MultiTaskWaiter.HandleableTaskDelegate handleableTask)
        {
            if (_openingActor == null) _openingActor = new Task.MultiTaskWaiter();
            _openingActor.AddTask(handleableTask);
        }

        // ----------------------------

        /// <summary>
        /// Close時に自動でCloseを呼ぶものとして登録する
        /// </summary>
        /// <param name="target">自動でCloseを呼びたい Manageable</param>
        /// <param name="autoStart">登録時にOpenを呼ぶか</param>
        protected void SetAutoCloser(Manageable target, bool autoStart = false)
        {
            if (target == null) return;
            _autoCloser.Add(target);
            if (autoStart) target.Open();
        }
    }
}
