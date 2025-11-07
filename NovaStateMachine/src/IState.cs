using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NovaStateMachine
{
    public interface IState
    {
        /// <summary> ステートの初期化処理を行う </summary>
        void Awake();

        /// <summary> ステートに突入したときの処理を行う </summary>
        void Enter();

        /// <summary> ステートの更新処理を行う </summary>
        void Update();

        /// <summary> ステートから退出するときの処理を行う </summary>
        void Exit();
    }
}
