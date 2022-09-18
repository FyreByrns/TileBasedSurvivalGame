namespace TileBasedSurvivalGame.StateMachines {
    abstract class State<TContext> {
        public IStateMachine<TContext> Owner { get; set; }
        public virtual void Enter() { }
        public virtual void Update(TContext context) { }
        public virtual void Exit() { }

        protected State(IStateMachine<TContext> owner) {
            Owner = owner;
        }
    }
}
