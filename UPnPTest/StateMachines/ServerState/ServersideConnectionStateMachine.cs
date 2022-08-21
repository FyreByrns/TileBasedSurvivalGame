using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FSerialization;

using TileBasedSurvivalGame.Networking;
using TileBasedSurvivalGame.StateMachines;

using SCM = TileBasedSurvivalGame.StateMachines.ServersideConnectionState.ServersideConnectionStateMachine;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

namespace TileBasedSurvivalGame.StateMachines.ServersideConnectionState {
    class ServersideConnectionStateMachine 
        : FSM<NetMessage, States.SS_ConnectionState> {

        public Server Server { get; }

        public ServersideConnectionStateMachine(Server server) {
            Server = server;
        }
    }


}
