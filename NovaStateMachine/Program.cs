using System;
using NovaStateMachine;
using System.Diagnostics;
using NovaStateMachine.TrafficSignalExsample;

namespace NovaStateMachine
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("サンプルコードを実行します");
            Console.WriteLine("");
            Console.WriteLine("どのサンプルを実行しますか?");
            Console.WriteLine("1. 信号機の実行");
            Console.WriteLine("2. アカウントの登録やログインの実行");
            string command = Console.ReadLine()?.Trim() ?? string.Empty;
            int.TryParse(command, out int commandNum);
            string commandName = string.Empty;
            var bootState = new BootStateMachine();
            switch (commandNum)
            {
                case 1:
                    commandName = "信号機";
                    bootState.SetStateMachine(new TrafficSignalStateMachine());
                    break;
                case 2:
                    commandName = "アカウント登録やログイン";
                    break;
                default:
                    Console.WriteLine("不明なコマンドです");
                    return;
            }

            Console.WriteLine($"コマンド: {commandName} を実行します");
            await UpdateStateMachines(bootState);
        }

        private static async Task UpdateStateMachines(BootStateMachine bootState)
        {
            using var cts = new CancellationTokenSource();
            var sw = Stopwatch.StartNew();
            long lastTicks = sw.ElapsedTicks;
            long frameTicks = Stopwatch.Frequency / 60; // 60fps

            while (true)
            {
                long nowTicks = sw.ElapsedTicks;
                long deltaTicks = nowTicks - lastTicks;
                // Console.WriteLine($"経過時間: {deltaTicks} ticks");

                if (deltaTicks >= frameTicks)
                {
                    long ms = (long)Math.Floor(deltaTicks * 1000.0 / Stopwatch.Frequency);
                    lastTicks = nowTicks;
                    bootState.Update(ms); // 1フレーム分の処理
                }
                else
                {
                    await Task.Delay(30, cts.Token);
                }

                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true); // 入力を消費
                    break;
                }
            }

            (bootState as IDisposable).Dispose();

            Console.WriteLine("処理を終了します");
        }
    }
}
