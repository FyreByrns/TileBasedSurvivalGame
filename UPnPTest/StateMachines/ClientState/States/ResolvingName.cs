using System.Collections.Generic;
using System.Linq;

using FSerialization;

using NetMessage = TileBasedSurvivalGame.Networking.NetMessage;
using CSM = TileBasedSurvivalGame.StateMachines.ClientsideConnectionState.ClientsideConnectionStateMachine;

using static TileBasedSurvivalGame.Networking.NetMessage.Intent;
using TileBasedSurvivalGame.Networking;

namespace TileBasedSurvivalGame.StateMachines.ClientsideConnectionState.States {
    class ResolvingName
        : CS_ConnectionState {
        public override void Enter(NetMessage context) {
            string name = ((CSM)StateMachine).Client.StringPopup("enter desired username");
            List<byte> data = new List<byte>();
            data.Append(name);

            NetHandler.SendToServer(NetMessage.ConstructToSend(RequestDesiredName, data.ToArray()));
        }

        public override IState<NetMessage> Update(NetMessage context) {
            if(context.MessageIntent == AllowConnection) {
                return new InLobby();
            }
            if(context.MessageIntent == DenyDesiredName) {
                return new ResolvingName();
            }
            return base.Update(context);
        }
    }

    class InLobby
        : CS_ConnectionState {
        public override void Enter(NetMessage context) {
            System.Console.WriteLine("reached lobby state!!!");
        }
    }
}
