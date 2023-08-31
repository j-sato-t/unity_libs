namespace UniLib.Core
{
    public class Checker
    {
        /// <summary>
        /// 複数をまとめてNullチェックする
        /// </summary>
        /// <param name="targets">チェックする対象</param>
        /// <returns>すべてがNotNullであるか</returns>
        public static bool NoNullAll(params object[] targets)
        {
            foreach (object target in targets)
            {
                if (target == null) return false;
            }
            return true;
        }
    }
}
