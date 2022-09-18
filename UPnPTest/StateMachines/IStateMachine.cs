namespace TileBasedSurvivalGame.StateMachines {
    interface IStateMachine<TContext> {
        State<TContext> CurrentState { get; set; }
        State<TContext> NextState { get; set; }
    }
}
