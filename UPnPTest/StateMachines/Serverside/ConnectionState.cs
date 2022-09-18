using FSerialization;
using TileBasedSurvivalGame.Networking;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

//// SERVERSIDE

namespace TileBasedSurvivalGame.StateMachines.Serverside {
    class ConnectionState : State<NetMessage> {
        public ConnectionState(ConnectionStateMachine owner)
            : base(owner) { }
    }

    namespace ConnectionStates {
        class ResolvingName :
            ConnectionState {
            public override void Update(NetMessage context) {
                if (context.MessageIntent == RequestDesiredName) {
                    string desiredName = context.RawData.Get<string>();
                    // if the name is allowed
                    if (ReservedWords.WordIsAllowed(desiredName)) {
                        NetHandler.SendToClient(context.Sender, NetMessage.ConstructToSend(AllowDesiredName));
                    }
                    else {
                        NetHandler.SendToClient(context.Sender, NetMessage.ConstructToSend(DenyDesiredName));
                    }
                }

                // if the client received permission
                if (context.MessageIntent == Received) {
                    // enter lobby state with the client
                    Owner.CurrentState = new InLobby((ConnectionStateMachine)Owner);
                }
            }

            public ResolvingName(ConnectionStateMachine owner) : base(owner) { }
        }

        class InLobby
            : ConnectionState {
            public InLobby(ConnectionStateMachine owner) : base(owner) { }
        }
    }
}
