using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FSerialization;

using NetMessage = TileBasedSurvivalGame.Networking.NetMessage;
using CSM = TileBasedSurvivalGame.StateMachines.ClientsideConnectionState.ClientsideConnectionStateMachine;

using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

namespace TileBasedSurvivalGame.StateMachines.ClientsideConnectionState {
    class ClientsideConnectionStateMachine
        : FSM<NetMessage, CS_ConnectionState> {
        public Networking.Client Client { get; }

        public ClientsideConnectionStateMachine(Networking.Client client) {
            Client = client;
            Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(NetMessage message) {
            Update(message, this);
        }
    }

    abstract class CS_ConnectionState
        : IState<NetMessage> {
        public IStateMachine<NetMessage> StateMachine { get; set; }

        public virtual void Enter(NetMessage context) { }
        public virtual IState<NetMessage> Update(NetMessage context) { return null; }
        public virtual void Exit(NetMessage context) { }
    }

    class InitiatingConnection
        : CS_ConnectionState {
        public override void Enter(NetMessage context) {
            ((CSM)StateMachine).Client.SendToServer(NetMessage.ConstructToSend(RequestConnection));
        }

        public override IState<NetMessage> Update(NetMessage context) {
            if (context.MessageIntent == AllowConnection) {
                return new ResolvingName();
            }
            return null; // stay in the current state
        }
    }

    class ResolvingName
        : CS_ConnectionState {
        public override void Enter(NetMessage context) {
            string name = ((CSM)StateMachine).Client.StringPopup("enter desired username");
            List<byte> data = new List<byte>();
            data.Append(name);

            ((CSM)StateMachine).Client.SendToServer(NetMessage.ConstructToSend(RequestDesiredName, data.ToArray()));
        }
    }
}
