using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.StateMachines {
    static class StateMachineHandler {
        public static void Update<TContext>(IStateMachine<TContext> machine, TContext context) {
            if (machine.CurrentState == null) {
                return;
            }

            machine.CurrentState.Update(context);
            // if the state changed
            if (machine.NextState != null && machine.CurrentState.GetType() != machine.NextState.GetType()) {
                // exit the current state
                machine.CurrentState.Exit();
                // enter the new state
                machine.NextState.Enter();
                // set the current state to the new state
                machine.CurrentState = machine.NextState;
            }
        }
    }
}
