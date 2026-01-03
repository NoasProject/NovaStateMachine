#if UNITY_EDITOR
using UnityEngine;
using NovaStateMachine;
using System.Collections.Generic;

namespace NovaStateMachine.Extensions
{
    public sealed class StateMachineTracker
    {
        private static StateMachineTracker _instance;
        public static StateMachineTracker Instance => _instance ??= new StateMachineTracker();

        public HashSet<BootStateMachine> trackedStateMachines = new HashSet<BootStateMachine>();

        public StateMachineTracker()
        {
            Application.quitting += () => _instance = null;
        }

        /// <summary>
        /// 監視対象として追加する
        /// </summary>
        public void AddTracker(BootStateMachine bootStateMachine)
        {
            this.trackedStateMachines.Add(bootStateMachine);
        }

        public void RemoveTracker(BootStateMachine bootStateMachine)
        {
            this.trackedStateMachines.Remove(bootStateMachine);
        }
    }
}
#endif
