namespace UniLib.Core
{
    public class Progressable : Manageable
    {
        public class ProgressableSetting : ManageableSetting
        {
            public Manageable WaitTarget;
            internal Condition TargetCondition = Condition.Running;
        }
        protected new ProgressableSetting Setting => base.Setting as ProgressableSetting;


        // ------------------------------------------------

        public bool IsFail => Setting.WaitTarget.NowCondition == Condition.Failed;

        public bool IsSuccess => Setting.WaitTarget.NowCondition == Setting.TargetCondition;

        // ================================================

        // ================================================

        public void Update(float deltaMs)
        {
            // 処理が終わったら自身を終了する
            if (IsSuccess)
            {
                Close();
                return;
            }

            // 失敗の判定
            if (IsFail)
            {
                SetFailed();
                return;
            }
        }
    }
}
