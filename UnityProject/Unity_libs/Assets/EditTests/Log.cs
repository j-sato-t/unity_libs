using System;
using NUnit.Framework;
using UniLib.Log;
using UnityEngine;
using UnityEngine.TestTools;

public class Log
{
    // A Test behaves as an ordinary method
    [Test]
    public void NomalLogTest()
    {
        UniLib.Log.LogHandler.SetupLogHandler();

        Debug.Log("test log Log");
        Debug.LogWarning("test log Warning");

        // エラーログを失敗扱いにしない
        LogAssert.ignoreFailingMessages = true;
        Debug.LogError("test log Error");
        Debug.LogAssertion("test log Assertion");
        LogAssert.ignoreFailingMessages = false;
    }

    [Test]
    public void ExceptionLogTest()
    {
        UniLib.Log.LogHandler.SetupLogHandler();

        // エラーログを失敗扱いにしない
        LogAssert.ignoreFailingMessages = true;
        Debug.LogAssertion(new Exception("test exception"));
        LogAssert.ignoreFailingMessages = false;
    }


    [TestCase(UniLib.Log.LogLevel.Trace)]
    [TestCase(UniLib.Log.LogLevel.Debug)]
    [TestCase(UniLib.Log.LogLevel.Info)]
    [TestCase(UniLib.Log.LogLevel.Warning)]
    [TestCase(UniLib.Log.LogLevel.Error)]
    [TestCase(UniLib.Log.LogLevel.Critical)]
    public void LoggerTest(UniLib.Log.LogLevel level)
    {
        UniLib.Log.Logger logger = new UniLib.Log.Logger(level);

        // エラーログを失敗扱いにしない
        LogAssert.ignoreFailingMessages = true;
        logger.LogTrace("trace");
        logger.LogDebug("debug");
        logger.LogInfo("info");
        logger.LogWarning("warning");
        logger.LogError("error");
        logger.LogCritical("critical");
        LogAssert.ignoreFailingMessages = false;
    }
}
