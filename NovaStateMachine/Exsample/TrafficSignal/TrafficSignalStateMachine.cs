using System;
using System.Collections.Generic;
using System.Linq;
using NovaStateMachine.TrafficSignalExsample;

namespace NovaStateMachine.TrafficSignalExsample
{
    public class TrafficSignalContext
    {
        public TrafficSignalType Signal { get; set; }
        public int loopCount = 0;
    }

    public interface ITrafficSignalStateMachine
    {
        TrafficSignalContext Context { get; }
    }

    public class TrafficSignalStateMachine : StateMachine, ITrafficSignalStateMachine
    {
        private TrafficSignalContext _context = new TrafficSignalContext();
        public TrafficSignalContext Context => this._context;

        public TrafficSignalStateMachine()
        {
            // ステートの追加処理とインスタンスの生成
            this.AddState<GreenState>();
            this.AddState<YellowState>();
            this.AddState<RedState>();

            // 遷移の情報を設定する
            this.AddTransition<GreenState, YellowState>("ToNext");
            this.AddTransition<YellowState, RedState>("ToNext");
            this.AddTransition<RedState, GreenState>("ToNext");

            // 最初は緑色に設定する
            this.SetInitialState<GreenState>();
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            Console.WriteLine("【TrafficSignalStateMachine】信号機のステートマシーンを初期化します");
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            Console.WriteLine("【TrafficSignalStateMachine】信号機のステートマシーンを起動します");
        }

        protected override void OnUpdate(long elapsedMs)
        {
            base.OnUpdate(elapsedMs);
        }

        protected override void OnExit()
        {
            base.OnExit();
            Console.WriteLine("【TrafficSignalStateMachine】信号機のステートマシーンを終了します");
        }
    }
}
