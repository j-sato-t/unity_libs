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
        private enum Condition
        {
            /// <summary>
            /// 生成された状態
            /// </summary>
            Created,
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
        }
        private Condition _condition;

        /// <summary>
        /// このインスタンスで使うロガー
        /// </summary>
        protected Log.Logger Logger { get; private set; }

        private ManageableSetting _setting;
        /// <summary>
        /// 設定へのアクセス<br>継承した設定クラスはこれをキャストする
        /// </summary>
        protected ManageableSetting Setting => _setting;


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

            if (OnOpen())
            {
                SetCondition(Condition.Running);
                return true;
            }
            else
            {
                SetCondition(Condition.Finished);
                Logger.LogError("fail Open");
                return false;
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

    }
}
