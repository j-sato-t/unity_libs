using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LogHandler
{
    // A Test behaves as an ordinary method
    [Test]
    public void NomalLogTest()
    {
        UniLib.LogHandler.SetupLogHandler();

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
        UniLib.LogHandler.SetupLogHandler();

        // エラーログを失敗扱いにしない
        LogAssert.ignoreFailingMessages = true;
        Debug.LogAssertion(new Exception("test exception"));
        LogAssert.ignoreFailingMessages = false;
    }
}
