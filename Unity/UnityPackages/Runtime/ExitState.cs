using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NovaStateMachine
{
    /// <summary>
    /// StateMachineから抜けるためのステート
    /// </summary>
    public class ExitState : State
    {
        public ExitState() { }

        protected override void OnAwake() { }

        protected override void OnEnter()
        {
            (this.StateMachine as IState).Exit();
        }

        protected override void OnUpdate(long elapsedMs) { }

        protected override void OnExit() { }
    }
}
