using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NovaStateMachine
{
    /// <summary>
    /// ステートの既定クラス
    /// </summary>
    public class State : IState
    {
        private StateMachine _stateMachine;
        protected StateMachine StateMachine => this._stateMachine;

        private bool _isAwaked = false;
        public bool IsActive { get; private set; }


        public State()
        {
        }

        // -------------------------------------------------------------
        // Stateの関数の定義
        // -------------------------------------------------------------
        protected virtual void OnAwake() { }

        protected virtual void OnEnter() { }

        protected virtual void OnExit() { }
        protected virtual void OnUpdate(long elapsedMs) { }

        protected bool Transition(string toState)
        {
            return this._stateMachine.TransitionInternal(toState);
        }

        protected bool Transition<T>() where T : State
        {
            return this._stateMachine.TransitionInternal(typeof(T).FullName);
        }

        // -------------------------------------------------------------
        // IStateの実装
        // -------------------------------------------------------------
        void IState.Init(StateMachine stateMachine)
        {
            this._stateMachine = stateMachine;
        }

        void IState.Enter()
        {
            // すでにアクティブのため実行しない
            if (this.IsActive) return;

            if (!this._isAwaked)
            {
                this._isAwaked = true;
                OnAwake();                
            }

            this.IsActive = true;
            OnEnter();
        }

        void IState.Update(long elapsedMs)
        {
            OnUpdate(elapsedMs);
        }

        void IState.Exit()
        {
            if (!this.IsActive) return;

            this.IsActive = false;
            OnExit();
        }
    }
}
