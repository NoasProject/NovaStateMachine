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
        private bool _isActive = false;
        public virtual bool IsActive => this._isActive;


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
            if (this._isActive) return;

            if (!this._isAwaked)
            {
                this._isAwaked = true;
                OnAwake();                
            }

            this._isActive = true;
            OnEnter();
        }

        void IState.Update(long elapsedMs)
        {
            OnUpdate(elapsedMs);
        }

        void IState.Exit()
        {
            if (!this._isActive) return;

            this._isActive = false;
            OnExit();
        }
    }
}
