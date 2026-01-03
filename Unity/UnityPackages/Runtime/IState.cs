using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NovaStateMachine
{
    internal interface IState
    {
        void Init(StateMachine stateMachine);

        /// <summary> ステートに突入したときの処理を行う </summary>
        void Enter();

        /// <summary> ステートの更新処理を行う </summary>
        void Update(long elapsedMs);

        /// <summary> ステートから退出するときの処理を行う </summary>
        void Exit();
    }
}
