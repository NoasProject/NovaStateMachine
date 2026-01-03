# NovaStateMachine

シンプルで軽量なC#用ステートマシンライブラリです。
Unityや.NETアプリケーションなど、様々なC#プロジェクトに簡単に組み込むことができます。

## 特徴

- **シンプルなAPI:** 直感的なAPIで簡単にステートマシンを構築できます。
- **階層型ステートマシン:** ステートマシン自体をステートとして扱うことで、複雑な状態管理を整理できます。
- **ライフサイクルイベント:** 各ステートには `OnAwake`, `OnEnter`, `OnUpdate`, `OnExit` といったライフサイクルイベントがあり、状態遷移に応じた処理を柔軟に実装できます。
- **型安全:** ジェネリクスを利用して、型安全にステートや遷移を定義できます。

## Unity (UPM) での導入

Unity Package Manager の「Add package from git URL...」に以下を指定してください。

```
https://github.com/NoasProject/NovaStateMachine.git?path=Unity/Packages
```

## 使い方

### 1. ステートを定義する

`State`クラスを継承して、個々の状態を定義します。
`OnEnter` `OnUpdate` `OnExit` などのメソッドをオーバーライドして、各状態での処理を記述します。
状態遷移は `Transition()` メソッドを呼び出すことで行います。

```csharp
// GreenState.cs
using NovaStateMachine;
using System;

public class GreenState : State
{
    private long _timer;

    protected override void OnEnter()
    {
        _timer = 3000; // 3秒
        Console.WriteLine("信号が緑になりました。");
    }

    protected override void OnUpdate(long elapsedMs)
    {
        _timer -= elapsedMs;
        if (_timer <= 0)
        {
            // "ToYellow" という名前の遷移をトリガー
            Transition("ToYellow");
        }
    }

    protected override void OnExit()
    {
        Console.WriteLine("信号が黄色に変わります。");
    }
}
```

### 2. ステートマシンを定義する

`StateMachine`クラスを継承して、ステートマシンを定義します。
コンストラクタ内で、使用するステートと遷移ルールを登録します。

- `AddState<T>()`: ステートを登録します。
- `AddTransition<TFrom, TTo>("TransitionName")`: あるステートから別のステートへの遷移を名前付きで登録します。
- `SetInitialState<T>()`: 初期状態を設定します。

```csharp
// TrafficSignalStateMachine.cs
using NovaStateMachine;

public class TrafficSignalStateMachine : StateMachine
{
    public TrafficSignalStateMachine()
    {
        // 1. ステートを登録
        AddState<GreenState>();
        AddState<YellowState>();
        AddState<RedState>();

        // 2. 遷移ルールを登録
        AddTransition<GreenState, YellowState>("ToYellow");
        AddTransition<YellowState, RedState>("ToRed");
        AddTransition<RedState, GreenState>("ToGreen");

        // 3. 初期状態を設定
        SetInitialState<GreenState>();
    }
}
```

### 3. ステートマシンを実行する

作成したステートマシンをインスタンス化し、`Enter()`メソッドで開始します。
`Update()`メソッドを定期的に呼び出すことで、ステートマシンが更新されます。

```csharp
// Program.cs
using System;
using System.Threading;

public class Program
{
    public static void Main(string[] args)
    {
        // ステートマシンを初期化
        var stateMachine = new TrafficSignalStateMachine();

        // ステートマシンを開始
        ((IState)stateMachine).Enter();

        // 5秒間、100ミリ秒ごとに更新
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        long lastElapsed = 0;

        for (int i = 0; i < 50; i++)
        {
            long currentElapsed = sw.ElapsedMilliseconds;
            ((IState)stateMachine).Update(currentElapsed - lastElapsed);
            lastElapsed = currentElapsed;
            Thread.Sleep(100);
        }

        // ステートマシンを終了
        ((IState)stateMachine).Exit();
    }
}
```
*※上記は実行例です。実際の`Program.cs`とは異なります。*

## License

MIT License

Copyright (c) 2024 noa
