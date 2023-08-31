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
    }


    public class Manageable
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

        /// <summary>
        /// このインスタンスで使うロガー
        /// </summary>
        protected Log.Logger Logger { get; private set; }

        private ManageableSetting _setting;
        /// <summary>
        /// 設定へのアクセス<br>継承した設定クラスはこれをキャストする
        /// </summary>
        protected ManageableSetting Setting => _setting;

        /// <summary>
        /// 完了を待つ開始処理
        /// <note>Finishedになるのを待つ</note>
        /// </summary>
        private List<Progressable> _openingAct = new List<Progressable>();


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

            if (!OnOpen())
            {
                // 失敗が確定した場合
                SetFailed();

                return false;
            }

            if (_openingAct.Count == 0)
            {
                SetCondition(Condition.Running);
                return true;
            }
            else
            {
                // 待ち処理開始
                foreach (var openAct in _openingAct)
                {
                    openAct.Open();
                }

                // 終了待機開始
                SetAutoCloser(Polling.UniTaskPolling.Instantiate(new Polling.UniTaskPolling.UniTaskPollingSetting
                {
                    PollingAction = (delta) =>
                    {
                        int finished = 0;
                        bool fail = false;
                        foreach (var act in _openingAct)
                        {
                            if (act.NowCondition == Condition.Running) act.Update(delta);
                            
                            if (act.IsSuccess) finished++;
                            else if (act.IsFail) fail = true;
                        }
                        if (fail)
                        {
                            SetFailed();
                            return false;
                        }
                        else if (finished == _openingAct.Count) {
                            SetCondition(Condition.Running);
                            return false;
                        }
                        return true;
                    }
                }), true);

                return true; // 開始自体は成功
            }
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

            if (OnResume())
            {
                SetCondition(Condition.Running);
                return true;
            }
            else
            {
                SetCondition(Condition.Finished);
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

            OnClose();

            // 自動終了
            foreach (var manageable in _autoCloser)
            {
                manageable?.Close();
            }

            SetCondition(Condition.Finished);
        }

        // ----------------------------
        /// <summary>
        /// 開始処理
        /// </summary>
        /// <returns>成功したか</returns>
        protected virtual bool OnOpen() { return true; }

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
