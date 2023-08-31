namespace UniLib.Core
{
    public interface ITickable
    {
        /// <summary>
        /// 周期処理
        /// </summary>
        /// <param name="deltaMs">経過時間（ミリ秒）</param>
        public void Update(float deltaMs);
    }
}
