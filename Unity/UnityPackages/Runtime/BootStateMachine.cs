using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaStateMachine
{
    /// <summary>
    /// 開始に使う大元のStateMachine
    /// </summary>
    public sealed class BootStateMachine : IDisposable
    {
        private StateMachine _bootStateMachine;
        private IState _bootState => this._bootStateMachine;

        private bool _isDisposabled = false;

        public BootStateMachine()
        {
        }

        void IDisposable.Dispose()
        {
            // すでに破棄をしている場合は処理をしない
            if (this._isDisposabled) return;

            this._isDisposabled = true;

            // 破棄される際には、必ず実行する
            this._bootState.Exit();
        }

        public void SetStateMachine<T>() where T : StateMachine
        {
            var stateMachine = Activator.CreateInstance<T>();
            this.SetStateMachineInternal(stateMachine);
        }

        public void SetStateMachine<T>(T stateMachine) where T : StateMachine
        {
            this.SetStateMachineInternal(stateMachine);
        }

        private void SetStateMachineInternal<T>(T stateMachine) where T : StateMachine
        {
            this._bootStateMachine = stateMachine;
        }

        public void Update(long elapsedMs)
        {
            if (this._isDisposabled)
                return;
            
            // StateMachineがまだ設定されていない場合は、処理をしない
            if (this._bootStateMachine == null)
                return;

            // アクティブじゃない場合は、Entry処理を実行する
            if (!this._bootStateMachine.IsActive)
            {
                this._bootState.Enter();
                return;
            }

            // 以降はアップデート関数を呼び出すようにする
            this._bootState.Update(elapsedMs);
        }
    }
}
