using System;
using UnityEngine;

namespace UniLib
{
    /// <summary>
    /// 拡張ログ基底クラス
    /// </summary>
    public abstract class LogHandlerBase : ILogHandler
    {
        private readonly ILogHandler _logHandler;

        public LogHandlerBase(ILogHandler logHandler)
        {
            _logHandler = logHandler;
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            OnExceptionLog(exception);
            _logHandler.LogException(exception, context);
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            string formated = string.Format($"[{logType}] {format}", args);
            OnFormatedLog(formated);
            _logHandler.LogFormat(logType, context, formated);
        }

        /// <summary>
        /// フォーマット済ログを受け取る
        /// </summary>
        /// <param name="logString">埋め込みなどが終わった文字列</param>
        protected abstract void OnFormatedLog(string logString);

        /// <summary>
        /// 例外ログを受け取る
        /// </summary>
        /// <param name="exception">発生した例外</param>
        protected abstract void OnExceptionLog(Exception exception);

        /// <summary>
        /// ログハンドラを入れ替える<br>継承先でこれを使う静的メソッドをつくる
        /// </summary>
        /// <typeparam name="T">セットするハンドラクラス（すでに設定されていたら更新しない）</typeparam>
        protected static void SetupLogHandler<T>() where T : LogHandler
        {
            var now = Debug.unityLogger.logHandler;
            if (now.GetType().Equals(typeof(T))) return;
            var newHandler = (T)typeof(T).GetConstructor(new Type[] { typeof(ILogHandler) }).Invoke(new object[] { now });
            Debug.unityLogger.logHandler = newHandler;
        }
    }

    /// <summary>
    /// 拡張ログの追加処理のない実クラス
    /// </summary>
    public class LogHandler : LogHandlerBase
    {
        public LogHandler(ILogHandler logHandler) : base(logHandler)
        {
        }

        protected override void OnExceptionLog(Exception exception)
        {
        }

        protected override void OnFormatedLog(string logString)
        {
        }

        public static void SetupLogHandler() => SetupLogHandler<LogHandler>();
    }
}
