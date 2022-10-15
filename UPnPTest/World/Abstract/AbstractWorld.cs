using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    class AbstractWorld {
        public WorldNode Origin { get; set; }

        public AbstractWorld() {
            Origin = new WorldNode();
        }
    }
}
