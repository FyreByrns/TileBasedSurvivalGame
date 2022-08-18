using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FSerialization;

using TileBasedSurvivalGame.Networking;
using TileBasedSurvivalGame.StateMachines;

using SCM = TileBasedSurvivalGame.StateMachines.ServerState.ServersideConnectionStateMachine;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

namespace TileBasedSurvivalGame.StateMachines.ServerState {
    class ServersideConnectionStateMachine 
        : FSM<NetMessage, SS_ConnectionState> {

        public Server Server { get; }

        public ServersideConnectionStateMachine(Server server) {
            Server = server;
            Server.MessageReceived += Server_MessageReceived;
        }

        private void Server_MessageReceived(NetMessage message) {
            Update(message);
        }
    }

    abstract class SS_ConnectionState
    : IState<NetMessage, SCM, SS_ConnectionState> {
        public virtual void Enter(NetMessage context, SCM machine) { }
        public virtual IState<NetMessage, SCM> Update(NetMessage context, SCM machine) { return null; }
        public virtual void Exit(NetMessage context, SCM machine) { }
    }

    class InitiatingConnection 
        : SS_ConnectionState {
        public override IState<NetMessage, SCM> Update(NetMessage context, SCM machine) {
            if(context.MessageIntent == RequestDesiredName) {
                // figure out if the name is allowed
                string requestedName = context.RawData.Get<string>();
                if (ReservedWords.WordIsAllowed(requestedName)) {
                    machine.Server.Respond(context, NetMessage.ConstructToSend(AllowConnection));
                }
            }

            return null;
        }
    }
}
