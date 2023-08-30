using UnityEngine;

namespace UniLib.Log
{
    /// <summary>
    /// ログの呼び出しを拡張する（UnityEngine.ILogger ではない）
    /// </summary>
    public class Logger
    {
        public enum LogLevel
        {
            /// <summary>
            /// 動作中の変数確認等
            /// </summary>
            Trace = 0,
            /// <summary>
            /// 分岐などの情報等
            /// </summary>
            Debug,
            /// <summary>
            /// 一般的なログ
            /// </summary>
            Info,
            /// <summary>
            /// 異常が発生しているが動作はしている
            /// </summary>
            Warning,
            /// <summary>
            /// 異常が発生し、動作できていない
            /// </summary>
            Error,
            /// <summary>
            /// クラッシュ相当のエラーが発生している
            /// </summary>
            Critical,
        }

        // ---------------------------------------------

        /// <summary>
        /// フィルタ：これ以上のレベルのみ出力する
        /// </summary>
        public LogLevel FilterLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// ログ文字列の先頭にログレベルを追加するか
        /// </summary>
        public bool AddLevelTag { get; set; } = true;

        // =========================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">フィルタの初期状態</param>
        /// <param name="addLevelTag">ログ文字列の先頭にログレベルを追加するか</param>
        public Logger(LogLevel filter = LogLevel.Info, bool addLevelTag = true)
        {
            FilterLevel = filter;
        }

        // ---------------------------------------------

        private void Log(LogLevel logLevel, string format, params object[] args)
        {
            if (logLevel < FilterLevel) return;

            if (AddLevelTag)
            {
                format = $"[{logLevel}] {format}";
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.LogFormat(format, args);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarningFormat(format, args);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogErrorFormat(format, args);
                    break;
            }
        }

        public void LogTrace(string format, params object[] args)
        {
            Log(LogLevel.Trace, format, args);
        }

        public void LogDebug(string format, params object[] args)
        {
            Log(LogLevel.Debug, format, args);
        }

        public void LogInfo(string format, params object[] args)
        {
            Log(LogLevel.Info, format, args);
        }

        public void LogWarning(string format, params object[] args)
        {
            Log(LogLevel.Warning, format, args);
        }

        public void LogError(string format, params object[] args)
        {
            Log(LogLevel.Error, format, args);
        }

        public void LogCritical(string format, params object[] args)
        {
            Log(LogLevel.Critical, format, args);
        }

        // ---------------------------------------------
    }
}
