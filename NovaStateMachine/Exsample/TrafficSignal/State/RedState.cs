using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaStateMachine.TrafficSignalExsample
{
    public class RedState : State
    {
        private ITrafficSignalStateMachine trafficSignalStateMachine => this.StateMachine as ITrafficSignalStateMachine;
        private long _waitMilliseconds;

        public RedState()
        { }

        protected override void OnAwake()
        {
        }

        protected override void OnEnter()
        {
            this.trafficSignalStateMachine.Context.Signal = TrafficSignalType.Red;
            this._waitMilliseconds = 1500; // 1.5秒待機
            Console.WriteLine($"【RedState - OnEnter】 - {this._waitMilliseconds / 1000.0m}秒間 [赤信号] を表示します");
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
