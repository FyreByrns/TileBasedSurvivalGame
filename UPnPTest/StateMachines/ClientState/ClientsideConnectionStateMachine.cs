using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FSerialization;

using NetMessage = TileBasedSurvivalGame.Networking.NetMessage;
using CSM = TileBasedSurvivalGame.StateMachines.ClientState.ClientsideConnectionStateMachine;

using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

namespace TileBasedSurvivalGame.StateMachines.ClientState {
    class ClientsideConnectionStateMachine
        : FSM<NetMessage, CS_ConnectionState> {
        public Networking.Client Client { get; }

        public ClientsideConnectionStateMachine(Networking.Client client) {
            Client = client;
            Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(NetMessage message) {
            Update(message);
        }
    }

    abstract class CS_ConnectionState 
        : IState <NetMessage, CSM> {
        public virtual void Enter(NetMessage context, CSM machine) { }
        public virtual IState<NetMessage, CSM> Update(NetMessage context, CSM machine) { return null; }
        public virtual void Exit(NetMessage context, CSM machine) { }
    }

    class InitiatingConnection
        : CS_ConnectionState {
        public override void Enter(NetMessage context, CSM machine) {
            machine.Client.SendToServer(NetMessage.ConstructToSend(RequestConnection));
        }

        public override IState<NetMessage, CSM> Update(NetMessage context, CSM machine) {
            if(context.MessageIntent == AllowConnection) {
                return new ResolvingName();
            }
            return null; // stay in the current state
        }
    }

    class ResolvingName
        : CS_ConnectionState {
        public override void Enter(NetMessage context, CSM machine) {
            string name = machine.Client.StringPopup("enter desired username");
            List<byte> data = new List<byte>();
            data.Append(name);

            machine.Client.SendToServer(NetMessage.ConstructToSend(RequestDesiredName, data.ToArray()));
        }
    }
}
