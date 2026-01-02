using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaStateMachine.TrafficSignalExsample
{
    public class YellowState : State
    {
        private ITrafficSignalStateMachine trafficSignalStateMachine => this.StateMachine as ITrafficSignalStateMachine;
        private long _waitMilliseconds;
        
        public YellowState()
        { }

        protected override void OnAwake()
        {
        }

        protected override void OnEnter()
        {
            this.trafficSignalStateMachine.Context.Signal = TrafficSignalType.Yellow;
            this._waitMilliseconds = 2000; // 2秒待機
            Console.WriteLine($"【YellowState - OnEnter】 - {this._waitMilliseconds / 1000.0m}秒間 [黄信号] を表示します");
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
