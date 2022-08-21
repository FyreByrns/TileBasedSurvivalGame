
using NetMessage = TileBasedSurvivalGame.Networking.NetMessage;

using static TileBasedSurvivalGame.Networking.NetMessage.Intent;
using TileBasedSurvivalGame.Networking;

namespace TileBasedSurvivalGame.StateMachines.ClientsideConnectionState.States {
    class InitiatingConnection
        : CS_ConnectionState {

        public override IState<NetMessage> Update(NetMessage context) {
            if (context?.MessageIntent == AllowConnection) {
                return new ResolvingName();
            }
            else {
                NetHandler.SendToServer(NetMessage.ConstructToSend(RequestConnection));
            }
            return null; // stay in the current state
        }
    }
}
