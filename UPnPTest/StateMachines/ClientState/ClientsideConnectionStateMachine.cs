using System;
using System.Text;
using System.Threading.Tasks;

using NetMessage = TileBasedSurvivalGame.Networking.NetMessage;
using TileBasedSurvivalGame.Networking;

namespace TileBasedSurvivalGame.StateMachines.ClientsideConnectionState {
    class ClientsideConnectionStateMachine
        : FSM<NetMessage, States.CS_ConnectionState> {
        public Networking.Client Client { get; }

        public ClientsideConnectionStateMachine(Networking.Client client) {
            Client = client;
            NetHandler.ClientMessage += Client_MessageReceived;
        }

        private void Client_MessageReceived(NetMessage message) {
            Update(message, this);
        }
    }
}
