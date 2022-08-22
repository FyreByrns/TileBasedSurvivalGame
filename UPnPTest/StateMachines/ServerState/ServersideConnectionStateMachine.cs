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

        public override void Update(NetMessage context, IStateMachine<NetMessage> machine) {
            base.Update(context, machine);
            Console.WriteLine($"current server state: {CurrentState}");
        }

        public ServersideConnectionStateMachine(Server server) {
            Server = server;
        }
    }


}
