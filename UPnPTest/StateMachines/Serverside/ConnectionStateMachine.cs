namespace TileBasedSurvivalGame.StateMachines.Serverside {
    class ConnectionStateMachine :
        IStateMachine<Networking.NetMessage> {
        public State<Networking.NetMessage> CurrentState { get; set; }
        public State<Networking.NetMessage> NextState { get; set; }
    }
}
