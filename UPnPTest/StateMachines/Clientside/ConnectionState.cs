using TileBasedSurvivalGame.Networking;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

//// CLIENTSIDE

namespace TileBasedSurvivalGame.StateMachines.Clientside {
    class ConnectionState
        : State<NetMessage> {
        public ConnectionState(ConnectionStateMachine owner) : base(owner) { }
    }

    namespace ConnectionStates {
        class InitiatingConnection
            : ConnectionState {

            public override void Enter() {
                NetHandler.SendToServer(NetMessage.ConstructToSend(RequestConnection));
            }

            public override void Update(NetMessage context) {
                if(context.MessageIntent == AllowConnection) {
                    Owner.NextState = new ResolvingName((ConnectionStateMachine)Owner);
                }
            }

            public InitiatingConnection(ConnectionStateMachine owner) 
                : base(owner) {
                Enter();
            }
        }

        class ResolvingName
            : ConnectionState {

            public override void Update(NetMessage context) {
                if(context.MessageIntent == AllowDesiredName) {


                    NetHandler.SendToServer(NetMessage.ConstructToSend(Received));
                }
            }

            public ResolvingName(ConnectionStateMachine owner) : base(owner) {
            }
        }
    }
}
