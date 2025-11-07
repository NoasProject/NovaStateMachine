using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NovaStateMachine
{
    /// <summary>
    /// ステートの既定クラス
    /// </summary>
    public abstract class State : IState
    {
        protected StateMachine m_stateMachine { get; }

        public State(StateMachine stateMachine)
        {
            this.m_stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        // -------------------------------------------------------------
        // Stateの関数の定義
        // -------------------------------------------------------------
        public abstract void OnAwake();

        public abstract void OnEnter();

        public abstract void OnExit();

        public abstract void OnUpdate();


        // -------------------------------------------------------------
        // IStateの実装
        // -------------------------------------------------------------
        void IState.Awake()
        {
            OnAwake();
        }

        void IState.Enter()
        {
            OnEnter();
        }

        void IState.Update()
        {
            OnUpdate();
        }

        void IState.Exit()
        {
            OnExit();
        }
    }
}
