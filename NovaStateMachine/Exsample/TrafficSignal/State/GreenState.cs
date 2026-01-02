using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaStateMachine.TrafficSignalExsample
{
    public class GreenState : State
    {
        private ITrafficSignalStateMachine trafficSignalStateMachine => this.StateMachine as ITrafficSignalStateMachine;
        private long _waitMilliseconds;

        public GreenState()
        { }

        protected override void OnAwake()
        {
            base.OnAwake();
            Console.WriteLine("GreenState: OnAwake");
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            // 現在の信号の色を緑色に設定する
            this.trafficSignalStateMachine.Context.loopCount++;
            this.trafficSignalStateMachine.Context.Signal = TrafficSignalType.Green;
            this._waitMilliseconds = 5000; // 5秒待機
            Console.WriteLine($"【GreenState - OnEnter】 - {this._waitMilliseconds / 1000.0m}秒間 [緑信号] を表示します. {this.trafficSignalStateMachine.Context.loopCount}回目のループ");
        }

        protected override void OnUpdate(long elapsedMs)
        {
            base.OnUpdate(elapsedMs);

            this._waitMilliseconds -= elapsedMs;
            if (_waitMilliseconds <= 0)
            {
                this.Transition("ToNext");
            }
        }
    }
}
