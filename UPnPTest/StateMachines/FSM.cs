namespace TileBasedSurvivalGame.StateMachines {
    interface IStateMachine<TContext> {
        void Update(TContext context, IStateMachine<TContext> machine);
    }

    //// finite state machine
    abstract class FSM<TContext> : IStateMachine<TContext> {
        public IState<TContext, IStateMachine<TContext>> CurrentState { get; protected set; }

        public void Update(TContext context, IStateMachine<TContext> machine) {
            IState<TContext, IStateMachine<TContext>> updateResult = CurrentState.Update(context, machine);
            if (updateResult != null) {
                CurrentState.Exit(context, machine);
                updateResult.Enter(context, machine);
                CurrentState = updateResult;
            }
        }
    }
}
