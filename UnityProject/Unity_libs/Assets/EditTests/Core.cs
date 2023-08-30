using NUnit.Framework;

public class Core
{
    // A Test behaves as an ordinary method
    [TestCase("")]
    [TestCase("TestTag")]
    public void Manageable_Base(string tag)
    {
        var manageable = UniLib.Core.Manageable.Instantiate<UniLib.Core.Manageable>(new UniLib.Core.ManageableSetting
        {
            FilterLevel = UniLib.Log.LogLevel.Trace,
            LogNameTag = tag,
        });
        Assert.IsNotNull(manageable);

        Assert.IsTrue(manageable.Open());
        Assert.IsTrue(manageable.Pause());
        Assert.IsTrue(manageable.Resume());
        manageable.Close();
    }
}
