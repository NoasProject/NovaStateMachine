using System;

namespace NovaStateMachine
{
    /// <summary>
    /// ステートを識別するための情報（名前・型・インスタンス）を保持
    /// </summary>
    public readonly struct StateIdentity : IEquatable<StateIdentity>
    {
        public string Name { get; }
        public Type StateType { get; }
        public State State { get; }

        public StateIdentity(string name, Type stateType, State state)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            this.State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public bool Equals(StateIdentity other)
        {
            return string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                && this.StateType == other.StateType
                && ReferenceEquals(this.State, other.State);
        }

        public override bool Equals(object obj)
        {
            return obj is StateIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StringComparer.Ordinal.GetHashCode(this.Name), this.StateType, this.State);
        }

        public static bool operator ==(StateIdentity left, StateIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StateIdentity left, StateIdentity right)
        {
            return !left.Equals(right);
        }
    }
}
