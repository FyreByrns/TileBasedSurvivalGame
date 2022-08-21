
using NetMessage = TileBasedSurvivalGame.Networking.NetMessage;

namespace TileBasedSurvivalGame.StateMachines.ClientsideConnectionState.States {
    abstract class CS_ConnectionState
        : IState<NetMessage> {
        public IStateMachine<NetMessage> StateMachine { get; set; }

        public virtual void Enter(NetMessage context) { }
        public virtual IState<NetMessage> Update(NetMessage context) { return null; }
        public virtual void Exit(NetMessage context) { }
    }
}
