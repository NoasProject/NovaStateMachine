using System;

namespace NovaStateMachine
{
    /// <summary>
    /// ステートを識別するための情報（名前・型・インスタンス）を保持
    /// </summary>
    internal readonly struct StateIdentity : IEquatable<StateIdentity>
    {
        public string Name { get; }
        public Type StateType { get; }
        public IState State { get; }
        public bool IsValid { get; }

        public StateIdentity(string name, Type stateType, IState state)
        {
            this.Name = name ?? string.Empty;
            this.StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            this.State = state ?? throw new ArgumentNullException(nameof(state));
            this.IsValid = true;
        }

        public bool Equals(StateIdentity other)
        {
            return this.IsValid == other.IsValid
                && string.Equals(this.Name, other.Name, StringComparison.Ordinal)
                && this.StateType == other.StateType
                && ReferenceEquals(this.State, other.State);
        }

        public override bool Equals(object obj)
        {
            return obj is StateIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            var nameHash = this.Name != null ? StringComparer.Ordinal.GetHashCode(this.Name) : 0;
            return HashCode.Combine(nameHash, this.StateType, this.State, this.IsValid);
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
