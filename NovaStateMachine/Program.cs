using System;
using NovaStateMachine.Battle;

namespace NovaStateMachine
{
    public class Program
    {
        public static void Main()
        {
            var context = BuildBattleContext();
            var stateMachine = new StateMachine<BattleContext>(context);

            stateMachine.AddState<BattleSetupState>();
            stateMachine.AddState<PlayerTurnState>();
            stateMachine.AddState<EnemyTurnState>();
            stateMachine.AddState<BattleResolutionState>();

            stateMachine.AddTransition<BattleSetupState, PlayerTurnState>();
            stateMachine.AddTransition<PlayerTurnState, EnemyTurnState>();
            stateMachine.AddTransition<EnemyTurnState, PlayerTurnState>();
            stateMachine.AddTransition<PlayerTurnState, BattleResolutionState>();
            stateMachine.AddTransition<EnemyTurnState, BattleResolutionState>();

            stateMachine.SetInitialState<BattleSetupState>();

            while (!context.SimulationCompleted)
            {
                stateMachine.Update();
            }

            Console.WriteLine("Simulation finished.");
        }

        private static BattleContext BuildBattleContext()
        {
            var context = new BattleContext
            {
                PlayerHp = 75,
                EnemyHp = 65
            };

            foreach (var damage in new[] { 14, 11, 18, 22 })
            {
                context.PlayerDamageScript.Enqueue(damage);
            }

            foreach (var damage in new[] { 9, 12, 17 })
            {
                context.EnemyDamageScript.Enqueue(damage);
            }

            return context;
        }
    }
}
