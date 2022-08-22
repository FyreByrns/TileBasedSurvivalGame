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

            foreach(byte b in data) {
                System.Console.Write((char)b);
            }
            System.Console.WriteLine();

            NetHandler.SendToServer(NetMessage.ConstructToSend(RequestDesiredName, data.ToArray()));
        }
    }
}
