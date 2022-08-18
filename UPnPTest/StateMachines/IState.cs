using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.StateMachines {
    interface IState<TContext> {
        IStateMachine<TContext> StateMachine { get; set; }
        void Enter(TContext context);
        IState<TContext> Update(TContext context);
        void Exit(TContext context);
    }
}
