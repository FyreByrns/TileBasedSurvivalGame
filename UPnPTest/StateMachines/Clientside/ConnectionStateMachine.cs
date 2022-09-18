using TileBasedSurvivalGame.Networking;

namespace TileBasedSurvivalGame.StateMachines.Clientside {
    class ConnectionStateMachine
        : IStateMachine<NetMessage> {
        public Client Owner { get; set; }

        public State<NetMessage> CurrentState { get; set; }
        public State<NetMessage> NextState { get; set; }

        public ConnectionStateMachine(Client owner) {
            Owner = owner;
        }
    }
}
