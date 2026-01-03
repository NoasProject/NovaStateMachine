using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NovaStateMachine;

namespace NovaStateMachine
{
    public interface IStateMachineTracker
    {
        IReadOnlyList<string> ChildStateNames { get; }
        State CurrentState { get; }
        State StartingState { get; }

        State GetState(string name);
        List<(string to, string transitionName)> GetTransitionsFromState(string fromStateName);
    }

    public partial class StateMachine : IStateMachineTracker
    {
        private List<string> _childStates = new List<string>();
        IReadOnlyList<string> IStateMachineTracker.ChildStateNames
        {
            get
            {
                this._childStates.Clear();
                foreach (var kvp in this._states)
                {
                    this._childStates.Add(kvp.Key);
                }
                return this._childStates;
            }
        }

        State IStateMachineTracker.CurrentState
        {
            get
            {
                if (this._currentStateIdentity.IsValid)
                {
                    return this._currentStateIdentity.State as State;
                }
                return null;
            }
        }

        State IStateMachineTracker.StartingState
        {
            get
            {
                if (this._startingStateIdentity.IsValid)
                {
                    return this._startingStateIdentity.State as State;
                }
                return null;
            }
        }

        /// <summary>
        /// 指定した名前のStateを取得する
        /// </summary>
        State IStateMachineTracker.GetState(string name)
        {
            if (this._states.TryGetValue(name, out var stateIdentity))
            {
                return stateIdentity.State as State;
            }
            return null;
        }

        List<(string to, string transitionName)> IStateMachineTracker.GetTransitionsFromState(string fromStateName)
        {
            var results = new List<(string to, string transitionName)>();
            if (this._transitions.TryGetValue(fromStateName, out var transitionIdentities))
            {
                foreach (var transitionIdentity in transitionIdentities)
                {
                    results.Add((transitionIdentity.To, transitionIdentity.Name));
                }
            }
            return results;
        }
    }
}
