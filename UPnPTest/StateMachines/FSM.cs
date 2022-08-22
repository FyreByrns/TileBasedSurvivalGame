namespace TileBasedSurvivalGame.StateMachines {
    interface IStateMachine<TContext> {
        void Update(TContext context, IStateMachine<TContext> machine);
    }

    //// finite state machine
    abstract class FSM<TContext, TState> : IStateMachine<TContext> 
        where TState : IState<TContext> {
        public TState CurrentState { get; set; }

        public void Enter() {
            CurrentState.StateMachine = this;
            CurrentState.Enter(default(TContext));
        }

        public virtual void Update(TContext context, IStateMachine<TContext> machine) {
            IState<TContext> updateResult = CurrentState.Update(context);
            if (updateResult != null) {
                CurrentState.Exit(context);
                updateResult.StateMachine = this;
                updateResult.Enter(context);
                CurrentState = (TState)updateResult;
            }
        }
    }
}
