using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedSurvivalGame.Networking;

namespace TileBasedSurvivalGame.StateMachines.ServersideConnectionState.States {
    abstract class SS_ConnectionState
: IState<NetMessage> {
        public IStateMachine<NetMessage> StateMachine { get; set; }

        public virtual void Enter(NetMessage context) { }
        public virtual IState<NetMessage> Update(NetMessage context) { return null; }
        public virtual void Exit(NetMessage context) { }
    }
}
