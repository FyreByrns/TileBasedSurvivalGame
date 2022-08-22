using FSerialization;
using TileBasedSurvivalGame.Networking;

using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

namespace TileBasedSurvivalGame.StateMachines.ServersideConnectionState.States {
    class NegotiatingName
        : SS_ConnectionState {
        public override IState<NetMessage> Update(NetMessage context) {
            if (context.MessageIntent == RequestDesiredName) {
                System.Console.WriteLine("client requesting name");

                // figure out if the name is allowed
                string requestedName = context.RawData.Get<string>();
                if (ReservedWords.WordIsAllowed(requestedName)) {
                    ((ServersideConnectionStateMachine)StateMachine)
                        .Server.Respond(context, NetMessage.ConstructToSend(AllowConnection));
                }
            }

            return null;
        }
    }
}
