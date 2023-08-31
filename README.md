# unity_libs

```puml
@startuml class_map

package UnityEngine{
    interface ILogHandler
    class MonoBehaviour
}

package UniLib{

package Log{
    abstract LogHandlerBase{
        - {readonly} ILogHandler _logHandler
        # ILogHandler ParentHandler{get}
        # {virtual} bool EnableAutoParent{get}
        + LogHandlerBase(ILogHandler logHandler)
        # {abstract} void OnFormatedLog(LogType logType, string logString)
        # {abstract} void OnExceptionLog(Exception exception)
        # {static} void SetupLogHandler<T>()
    }
    ILogHandler <|.. LogHandlerBase

    class LogHandler{
        + LogHandler(ILogHandler)
        + {static} void SetupLogHandler()
    }
    LogHandlerBase <|-- LogHandler

    enum LogLevel{
        Trace
        Debug
        Info
        Warning
        Error
        Critical
    }
    class Logger{
        + LogLevel FilterLevel{get;set;}
        + bool AddLevelTag{get;set;}
        + Logger(Loglevel filter, bool addLevelTag)
        - Log(Loglevel logLevel, string msg)
        + LogTrace ~ LogCritical(string msg)
    }
    Logger o-- LogLevel
}

package Core{
    enum Condition{
        Created
        Opening
        Running
        Pause
        Finished
        Failed
    }

    class ManageableSetting{
        + Log.LogLevel FilterLevel
    }

    class Manageable{
        - ManageableSetting Setting{}
        - Condition _condition
        + Condition NowCondition{get}
        - Logger _logger
        # Logger Logger{get}
        - List<Manageable> _autoCloser


        + {static} Manageable Instantiate(Setting setting)
        + bool Open()
        + bool Pause()
        + bool Resume()
        + void Close()
        + {virtual} bool OnOpen()
        + {virtual} void OnReady()
        + {virtual} bool OnPause()
        + {virtual} bool OnResume()
        + {virtual} void OnClose()
        # void SetFailed()
        # void SetAutoCloser(Manageable target, bool autoStart)
    }
    Manageable *-- Condition
    Manageable o-- Logger
    ManageableSetting --* Manageable
    ITickable <|.. Manageable

    interface ITickable{
        + void Update(float deltaMs)
    }
}

package Polling{
    class UniTaskPolling{
        # {delegate} PollingActionDelegate Setting.PollingAction
        # bool OnOpen()
        - UniTask TickLoop()
    }
    Manageable <|-- UniTaskPolling
}

package Task {
    class MultiTaskWaiter {
        - List<HandleableDelegate> _taskMethodList

        + void AddTask(HandleableDelegate taskMethod)
        + void StartWait(UnityAction<bool> resultCB)
        + UniTask<bool> StartWaitAsync()
        - UniTask RunTask(HandledDelegate taskMethod)
    }
}











}
@enduml
```

```puml
@startuml wait_opening

participant Manageable as mana
collections Progressable as prog
collections WaitTarget as wait

activate mana
    mana->>wait:生成
    mana->>prog:生成
    mana-->>prog:待機開始
    activate prog
        prog-->>wait:処理開始
        loop 周期処理
            prog->>wait:状態確認
            alt 終了
                prog->>prog:Close
            else 失敗
                prog->>prog:SetFail
            end
        end
    deactivate prog
    loop 周期処理
        mana->>prog:状態確認
        alt すべて成功
            mana->>mana:toRunning
        else 失敗 > 0
            mana->>mana:SetFail
        end
    end
deactivate mana

@enduml
```
