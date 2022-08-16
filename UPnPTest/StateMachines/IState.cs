using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.StateMachines {
    interface IState<TContext, TMachine>
        where TMachine : IStateMachine<TContext> {
        void Enter(TContext context, TMachine machine);
        IState<TContext, TMachine> Update(TContext context, TMachine machine);
        void Exit(TContext context, TMachine machine);
    }
}
