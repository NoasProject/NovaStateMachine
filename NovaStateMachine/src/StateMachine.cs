using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaStateMachine
{
    public partial class StateMachine : State, IState
    {
        // -------------------------------------------------------------
        // プライベート変数
        private bool _isAwaked = false;
        private bool _isActive = false;

        // -------------------------------------------------------------
        // プライベート構造体
        private readonly Dictionary<string, StateIdentity> _states;
        private readonly Dictionary<string, List<TransitionIdentity>> _transitions;
        private StateIdentity _startingStateIdentity;
        private StateIdentity _currentStateIdentity;
        private TransitionIdentity _nextTransitionIdentity;

        // -------------------------------------------------------------
        // プロパティ
        public override bool IsActive => this._isActive;
        protected StateMachine stateMachine { get; private set; }

        public StateMachine() : base()
        {
            this._states = new Dictionary<string, StateIdentity>();
            this._transitions = new Dictionary<string, List<TransitionIdentity>>();
            this._startingStateIdentity = default;
            this._currentStateIdentity = default;
            this._nextTransitionIdentity = default;
        }

        protected void Clear()
        {
            this._states.Clear();
            this._transitions.Clear();
            this._startingStateIdentity = default;
            this._currentStateIdentity = default;
            this._nextTransitionIdentity = default;
        }

        // -------------------------------------------------------------
        // IStateの実装
        // -------------------------------------------------------------
        void IState.Init(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        void IState.Enter()
        {
            if (this._isActive) return;

            if (!this._isAwaked)
            {
                this._isAwaked = true;
                OnAwake();
            }

            // 現在の子Stateが設定されていない場合は、開始ステートを設定する
            if (!this._currentStateIdentity.IsValid && this._startingStateIdentity.IsValid)
            {
                this._currentStateIdentity = this._startingStateIdentity;
            }

            this._isActive = true;
            this.OnEnter();

            // 子の処理を後に行う
            this._currentStateIdentity.State?.Enter();
        }

        void IState.Update(long elapsedMs)
        {
            // 次の遷移が設定されている場合は、遷移処理を行う
            if (this._nextTransitionIdentity != null)
            {
                this.TransitionProcess(this._nextTransitionIdentity);
                this._nextTransitionIdentity = null;
                return;
            }

            // 遷移が終わった後にUpdate処理を行う
            this._currentStateIdentity.State?.Update(elapsedMs);
            this.OnUpdate(elapsedMs);
        }

        void IState.Exit()
        {
            if (!this._isActive) return;

            this._isActive = false;
            // 子StateのExit処理を先に行う
            _currentStateIdentity.State.Exit();

            // 自身のExit処理を行う
            this.OnExit();
            // 現在のStateを破棄する
            this._currentStateIdentity = default;
        }

        // -------------------------------------------------------------
        // StateMachineの仮想関数
        // -------------------------------------------------------------
        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void OnEnter()
        {
            base.OnEnter();
        }

        protected override void OnExit()
        {
            base.OnExit();
        }

        /// <summary> 現在のステートを更新する </summary>
        protected override void OnUpdate(long elapsedMs)
        {
            base.OnUpdate(elapsedMs);
        }

        // -------------------------------------------------------------
        // Public関数
        // -------------------------------------------------------------

        /// <summary> Stateを追加する </summary>
        public void AddState<TState>() where TState : State
        {
            this.AddStateInternal<TState>(typeof(TState).FullName, default(TState));
        }

        /// <summary> Stateを追加する </summary>
        public void AddState<TState>(string stateName) where TState : State
        {
            this.AddStateInternal<TState>(stateName, default);
        }

        /// <summary> Stateを追加する </summary>
        public void AddState<TState>(string stateName, TState state) where TState : State
        {
            this.AddStateInternal(stateName, state);
        }

        private StateIdentity AddStateInternal<TState>(string stateName, TState state) where TState : IState
        {
            if (this._states.ContainsKey(stateName))
            {
                throw new InvalidOperationException($"State {stateName} is already registered.");
            }

            TState instance = state ?? CreateStateInstance<TState>();
            (instance as IState).Init(this);
            var stateIdentity = new StateIdentity(stateName, typeof(TState), instance);
            this._states.Add(stateName, stateIdentity);
            return stateIdentity;
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition<TFrom, TTo>(string transitionName) where TFrom : State where TTo : State
        {
            this.AddTransitionInternal(transitionName, typeof(TFrom).FullName, typeof(TTo).FullName, null);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition<TFrom, TTo, TTransition>(TTransition transitionValue) where TFrom : State where TTo : State where TTransition : Enum
        {
            this.AddTransitionInternal(TransitionIdentity.ToKey(transitionValue), typeof(TFrom).FullName, typeof(TTo).FullName, null);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition<TFrom, TTo>(string transitionName, Action callback) where TFrom : State where TTo : State
        {
            this.AddTransitionInternal(transitionName, typeof(TFrom).FullName, typeof(TTo).FullName, callback);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition<TFrom, TTo, TTransition>(TTransition transitionValue, Action callback) where TFrom : State where TTo : State where TTransition : Enum
        {
            this.AddTransitionInternal(TransitionIdentity.ToKey(transitionValue), typeof(TFrom).FullName, typeof(TTo).FullName, callback);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition(string transitionName, string fromState, string toState)
        {
            this.AddTransitionInternal(transitionName, fromState, toState, null);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition<TTransition>(TTransition transitionValue, string fromState, string toState) where TTransition : Enum
        {
            this.AddTransitionInternal(TransitionIdentity.ToKey(transitionValue), fromState, toState, null);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition(string transitionName, string fromState, string toState, Action callback)
        {
            this.AddTransitionInternal(transitionName, fromState, toState, callback);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition<TTransition>(TTransition transitionValue, string fromState, string toState, Action callback) where TTransition : Enum
        {
            this.AddTransitionInternal(TransitionIdentity.ToKey(transitionValue), fromState, toState, callback);
        }

        private void AddTransitionInternal(string transitionName, string fromState, string toState, Action callback)
        {
            EnsureStateRegistered(fromState);
            EnsureStateRegistered(toState);

            // 新しく配列を追加する
            if (!this._transitions.TryGetValue(fromState, out var transitions))
            {
                transitions = new List<TransitionIdentity>();
                this._transitions.Add(fromState, transitions);
            }

            // すでに遷移が登録されているかチェックする
            if (transitions.Any(t => t.To == toState))
            {
                throw new InvalidOperationException($"Transition from {fromState} to {toState} already exists.");
            }

            // 遷移先を登録する
            transitions.Add(new TransitionIdentity(transitionName, fromState, toState, callback));
        }

        /// <summary>
        /// 初期ステートを設定する
        /// </summary>
        public void SetInitialState<TState>() where TState : State
        {
            var stateIdentity = this._states.GetValueOrDefault(typeof(TState).FullName);
            this._startingStateIdentity = stateIdentity;
        }

        internal bool TransitionInternal(string transitionName, bool isForce = false)
        {
            // 現在設定されているStateが存在しない場合は処理をしない
            if (!this._currentStateIdentity.IsValid)
            {
                return false;
            }

            // すでに遷移が設定されている場合は処理をしない
            if (this._nextTransitionIdentity != null && !isForce)
            {
                return false;
            }

            var currentName = this._currentStateIdentity.Name;
            string toStateName = "";
            // 遷移の設定が存在しない場合は遷移不可
            if (!TryGetTransition(transitionName, currentName, toStateName, out var transition) || transition is null)
            {
                return false;
            }

            // 遷移を設定する
            this._nextTransitionIdentity = transition;
            return true;
        }

        private void TransitionProcess(TransitionIdentity transition)
        {
            if (transition == null)
                return;

            // 遷移先のStateを取得する
            var identity = this._states.GetValueOrDefault(transition.To);
            if (identity.State == null)
            {
                return;
            }

            // Stateのアクティブを有効化する
            this.Activate(identity);
        }

        /// <summary>
        /// ステートを有効化する
        /// </summary>
        private void Activate(StateIdentity nextStateIdentity)
        {
            // 同じStateの場合は処理をしない
            if (this._currentStateIdentity == nextStateIdentity)
            {
                return;
            }

            // 古いStateから抜ける
            var prevStateIdentity = this._currentStateIdentity;
            this._currentStateIdentity = default;
            prevStateIdentity.State?.Exit();

            // 新しいStateに入れ替える
            nextStateIdentity.State?.Enter();
            this._currentStateIdentity = nextStateIdentity;
        }

        private TState CreateStateInstance<TState>() where TState : IState
        {
            var instance = Activator.CreateInstance<TState>();
            if (instance == null)
            {
                throw new InvalidOperationException($"State {typeof(TState).FullName} must define a constructor that accepts {nameof(StateMachine)}.");
            }

            return instance;
        }

        /// <summary>
        /// Stateが登録されているかチェックする関数
        /// </summary>
        private void EnsureStateRegistered(string key)
        {
            if (!this._states.ContainsKey(key))
            {
                throw new InvalidOperationException($"State {key} is not registered. Call AddState<{key}>() first.");
            }
        }

        /// <summary>
        /// 遷移情報を取得する
        /// </summary>
        private bool TryGetTransition(string transitionName, string from, string to, out TransitionIdentity transition)
        {
            transition = null;
            if (!this._transitions.TryGetValue(from, out var transitions))
            {
                return false;
            }

            // 遷移が存在する場合は、それを返す
            foreach (var x in transitions)
            {
                // 名前での遷移か、遷移先での遷移かを確認する
                if (!string.IsNullOrWhiteSpace(transitionName))
                {
                    if (x.Name == transitionName)
                    {
                        transition = x;
                        return true;
                    }
                }
                else
                {
                    if (x.To == to)
                    {
                        transition = x;
                        return true;
                    }
                }
            }

            // 遷移が存在しない
            return false;
        }
    }
}
