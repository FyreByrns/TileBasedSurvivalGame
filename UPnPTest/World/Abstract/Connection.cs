using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    enum ConnectionType {
        Any,
        Land,
        Water,
    }

    class Connection {
        public ConnectionType ConnectionType { get; set; }

        public WorldNode A { get; set; }
        public WorldNode B { get; set; }

        public Connection(WorldNode a, WorldNode b) {
            A = a;
            B = b;
        }
    }
}
