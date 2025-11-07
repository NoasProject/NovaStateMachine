using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaStateMachine
{
    public class StateMachine<TContext> : StateMachine where TContext : class
    {
        public TContext Context { get; }

        public StateMachine(TContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }

    public class StateMachine
    {
        private sealed class Transition
        {
            public Type From { get; }
            public Type To { get; }
            public Func<bool>? Guard { get; }

            public Transition(Type from, Type to, Func<bool>? guard)
            {
                this.From = from;
                this.To = to;
                this.Guard = guard;
            }

            public bool CanTraverse()
            {
                return this.Guard?.Invoke() ?? true;
            }
        }

        private Dictionary<Type, State> m_states { get; }
        private Dictionary<Type, List<Transition>> m_transitions { get; }
        private State m_currentState { get; }

        public StateMachine()
        {
            this.m_states = new Dictionary<Type, State>();
            this.m_transitions = new Dictionary<Type, List<Transition>>();
        }

        public State? CurrentState => this.m_currentState;

        /// <summary>
        /// ステートを追加し、Awakeを一度だけ呼び出す
        /// </summary>
        public TState AddState<TState>() where TState : State
        {
            Type stateType = typeof(TState);
            if (this.m_states.ContainsKey(stateType))
            {
                throw new InvalidOperationException($"State {stateType.Name} is already registered.");
            }

            var instance = CreateStateInstance(stateType);
            this.m_states.Add(stateType, instance);
            instance.OnAwake();
            return (TState)instance;
        }

        /// <summary>
        /// ステート遷移を登録する。条件付きの遷移も定義可能。
        /// </summary>
        public void AddTransition<TFrom, TTo>() where TFrom : State where TTo : State
        {
            Type fromType = typeof(TFrom);
            Type toType = typeof(TTo);

            EnsureStateRegistered(fromType);
            EnsureStateRegistered(toType);

            if (!this.m_transitions.TryGetValue(fromType, out var transitions))
            {
                transitions = new List<Transition>();
                this.m_transitions.Add(fromType, transitions);
            }

            if (transitions.Any(t => t.To == toType))
            {
                throw new InvalidOperationException($"Transition from {fromType.Name} to {toType.Name} already exists.");
            }

            transitions.Add(new Transition(fromType, toType, guard));
        }

        /// <summary>
        /// 初期ステートを設定する
        /// </summary>
        public void SetInitialState<TState>() where TState : State
        {
            if (this.m_currentState != null)
            {
                throw new InvalidOperationException("Initial state is already set.");
            }

            var state = GetStateInstance(typeof(TState));
            Activate(state);
        }

        /// <summary>
        /// 遷移条件を確認してステートを変更する
        /// </summary>
        public bool TryChangeState<TState>() where TState : State
        {
            return TryChangeState(typeof(TState));
        }

        /// <summary>
        /// 現在のステートを更新する
        /// </summary>
        public void Update()
        {
            this.m_currentState?.OnUpdate();
        }

        private bool TryChangeState(Type targetType)
        {
            var targetState = GetStateInstance(targetType);

            if (this.m_currentState != null)
            {
                var currentType = this.m_currentState.GetType();
                if (!TryGetTransition(currentType, targetType, out var transition) || transition is null)
                {
                    return false;
                }

                if (!transition.CanTraverse())
                {
                    return false;
                }
            }

            Activate(targetState);
            return true;
        }

        private void Activate(State nextState)
        {
            if (this.m_currentState == nextState)
            {
                return;
            }

            this.m_currentState?.OnExit();
            this.m_currentState = nextState;
            this.m_currentState.OnEnter();
        }

        private State GetStateInstance(Type type)
        {
            if (!this.m_states.TryGetValue(type, out var state))
            {
                throw new InvalidOperationException($"State {type.Name} is not registered.");
            }

            return state;
        }

        private State CreateStateInstance(Type type)
        {
            var instance = Activator.CreateInstance(type, this) as State;
            if (instance == null)
            {
                throw new InvalidOperationException($"State {type.Name} must define a constructor that accepts {nameof(StateMachine)}.");
            }

            return instance;
        }

        private void EnsureStateRegistered(Type type)
        {
            if (!this.m_states.ContainsKey(type))
            {
                throw new InvalidOperationException($"State {type.Name} is not registered. Call AddState<{type.Name}>() first.");
            }
        }

        private bool TryGetTransition(Type from, Type to, out Transition? transition)
        {
            transition = null;
            if (!this.m_transitions.TryGetValue(from, out var transitions))
            {
                return false;
            }

            transition = transitions.FirstOrDefault(t => t.To == to);
            return transition is not null;
        }
    }
}
