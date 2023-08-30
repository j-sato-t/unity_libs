using UnityEngine;

namespace UniLib.Log
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

    /// <summary>
    /// ログの呼び出しを拡張する（UnityEngine.ILogger ではない）
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// フィルタ：これ以上のレベルのみ出力する
        /// </summary>
        public LogLevel FilterLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// ログ文字列の先頭にログレベルを追加するか
        /// </summary>
        public bool AddLevelTag { get; set; } = true;

        public string NameTag { get; set; } = "";

        // =========================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">フィルタの初期状態</param>
        /// <param name="addLevelTag">ログ文字列の先頭にログレベルを追加するか</param>
        public Logger(LogLevel filter = LogLevel.Info, string nameTag = "", bool addLevelTag = true)
        {
            FilterLevel = filter;
            NameTag = nameTag;
            AddLevelTag = addLevelTag;
        }

        // ---------------------------------------------

        private void Log(LogLevel logLevel, string msg)
        {
            if (logLevel < FilterLevel) return;

            if (!string.IsNullOrEmpty(NameTag))
            {
                msg = $"{{{NameTag}}} {msg}";
            }

            if (AddLevelTag)
            {
                msg = $"[{logLevel}] {msg}";
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(msg);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(msg);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError(msg);
                    break;
            }
        }

        public void LogTrace(string msg)
        {
            Log(LogLevel.Trace, msg);
        }

        public void LogDebug(string msg)
        {
            Log(LogLevel.Debug, msg);
        }

        public void LogInfo(string msg)
        {
            Log(LogLevel.Info, msg);
        }

        public void LogWarning(string msg)
        {
            Log(LogLevel.Warning, msg);
        }

        public void LogError(string msg)
        {
            Log(LogLevel.Error, msg);
        }

        public void LogCritical(string msg)
        {
            Log(LogLevel.Critical, msg);
        }

        // ---------------------------------------------
    }
}
