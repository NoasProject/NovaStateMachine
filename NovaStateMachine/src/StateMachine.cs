using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaStateMachine
{
    public class StateMachine<TContext> : StateMachine where TContext : IStateContext
    {
        public TContext Context { get; }

        public StateMachine(TContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }

    public partial class StateMachine : IState
    {
        private readonly Dictionary<string, StateIdentity> _states;
        private readonly Dictionary<string, List<Transition>> _transitions;
        private StateIdentity _currentIdentity;

        protected StateIdentity CurrentIdentity => this._currentIdentity;

        public IReadOnlyDictionary<string, StateIdentity> States => this._states;
        public State CurrentState => this._currentIdentity.State;

        public StateMachine()
        {
            this._states = new Dictionary<string, StateIdentity>();
            this._transitions = new Dictionary<string, List<Transition>>();
            this._currentIdentity = default;
        }

        public virtual void Awake()
        {
            
        }

        public virtual void Enter()
        {
            this._currentIdentity.State?.OnEnter();
        }

        public virtual void Exit()
        {
        }

        /// <summary> 現在のステートを更新する </summary>
        public virtual void Update()
        {
            this._currentIdentity.State?.OnUpdate();
        }

        /// <summary> Stateを追加する </summary>
        public TState AddState<TState>() where TState : State
        {
            return this.AddStateInternal<TState>(typeof(TState).FullName);
        }

        /// <summary> Stateを追加する </summary>
        public TState AddState<TState>(string stateName) where TState : State
        {
            return this.AddStateInternal<TState>(stateName);
        }

        public TState AddStateInternal<TState>(string stateName) where TState : State
        {
            if (this._states.ContainsKey(stateName))
            {
                throw new InvalidOperationException($"State {stateName} is already registered.");
            }

            var instance = CreateStateInstance<TState>();
            this._states.Add(stateName, new StateIdentity(stateName, typeof(TState), instance));
            return instance;
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition<TFrom, TTo>() where TFrom : State where TTo : State
        {
            this.AddTransitionInternal(typeof(TFrom).FullName, typeof(TTo).FullName);
        }

        /// <summary> ステート遷移を登録する </summary>
        public void AddTransition(string fromState, string toState)
        {
            this.AddTransitionInternal(fromState, toState);
        }

        private void AddTransitionInternal(string fromState, string toState)
        {
            EnsureStateRegistered(fromState);
            EnsureStateRegistered(toState);

            // 新しく配列を追加する
            if (!this._transitions.TryGetValue(fromState, out var transitions))
            {
                transitions = new List<Transition>();
                this._transitions.Add(fromState, transitions);
            }

            // すでに遷移が登録されているかチェックする
            if (transitions.Any(t => t.To == toState))
            {
                throw new InvalidOperationException($"Transition from {fromState} to {toState} already exists.");
            }

            // 遷移先を登録する
            transitions.Add(new Transition(fromState, toState));
        }

        /// <summary>
        /// 初期ステートを設定する
        /// </summary>
        public void SetInitialState<TState>() where TState : State
        {
            if (this._currentIdentity.State != null)
            {
                throw new InvalidOperationException("Initial state is already set.");
            }

            var state = this._states.GetValueOrDefault(typeof(TState).FullName);
            Activate(state);
        }

        /// <summary>
        /// 遷移条件を確認してステートを変更する
        /// </summary>
        public bool TryChangeState<TState>() where TState : State
        {
            return this.TryChangeStateInternal(typeof(TState).FullName);
        }

        /// <summary>
        /// 遷移条件を確認してステートを変更する
        /// </summary>
        public bool TryChangeState(string stateName)
        {
            return this.TryChangeStateInternal(stateName);
        }

        private bool TryChangeStateInternal(string stateName)
        {
            // 対象のStateを取得する
            var identity = this._states.GetValueOrDefault(stateName);

            // 現在設定されているStateが存在する場合は、遷移設定を確認する
            if (this._currentIdentity.State != null)
            {
                var currentName = this._currentIdentity.Name;
                // 遷移の設定が存在しない場合は遷移不可
                if (!TryGetTransition(currentName, stateName, out var transition) || transition is null)
                {
                    return false;
                }
            }

            this.Activate(identity);
            return true;
        }

        /// <summary>
        /// ステートを有効化する
        /// </summary>
        protected void Activate(StateIdentity nextStateIdentity)
        {
            // 同じStateの場合は処理をしない
            if (this._currentIdentity == nextStateIdentity)
            {
                return;
            }

            // 古いStateから抜ける
            var prevStateIdentity = this._currentIdentity;
            this._currentIdentity = default;
            prevStateIdentity.State?.OnExit();

            // 新しいStateに入れ替える
            nextStateIdentity.State?.OnEnter();
            this._currentIdentity = nextStateIdentity;
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
        private bool TryGetTransition(string from, string to, out Transition transition)
        {
            transition = null;
            if (!this._transitions.TryGetValue(from, out var transitions))
            {
                return false;
            }

            // 遷移が存在する場合は、それを返す
            foreach (var x in transitions)
            {
                if (x.To == to)
                {
                    transition = x;
                    return true;
                }
            }

            // 遷移が存在しない
            return false;
        }
    }
}
