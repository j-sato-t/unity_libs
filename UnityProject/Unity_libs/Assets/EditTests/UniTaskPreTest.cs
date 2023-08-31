using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

public class UniTaskPreTest
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TaskVarTest()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;

        var task = TestTask();

        while (!task.Status.IsCompleted()) yield return null;
        Debug.Log("after main while");

        task = TaskHandle(TestTask);
        while (!task.Status.IsCompleted()) yield return null;
        Debug.Log("after handled while");
    }

    private async Cysharp.Threading.Tasks.UniTask<bool> TestTask()
    {
        Debug.Log("befor test wait");
        await Cysharp.Threading.Tasks.UniTask.WaitForSeconds(1f);
        Debug.Log("after test wait");
        return true;
    }

    delegate UniTask<bool> HandledTaskMethod();

    private async UniTask<bool> TaskHandle(HandledTaskMethod taskMethod)
    {
        var result = await taskMethod();
        Debug.Log($"result: {result}");
        return true;
    }
}
