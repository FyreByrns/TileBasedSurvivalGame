using FSerialization;
using TileBasedSurvivalGame.Networking;

using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

namespace TileBasedSurvivalGame.StateMachines.ServersideConnectionState.States {
    class InitiatingConnection
        : SS_ConnectionState {
        public override IState<NetMessage> Update(NetMessage context) {
            if (context.MessageIntent == RequestConnection) {
                ((ServersideConnectionStateMachine)StateMachine)
                    .Server.Respond(context, NetMessage.ConstructToSend(AllowConnection));

                System.Console.WriteLine("allowing client connection");
                return new NegotiatingName();
            }
            return null;
        }
    }
}
