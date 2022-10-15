using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    enum WorldNodeType {
        None,

        PointOfInterest,
        Path,
    }

    class WorldNode {
        public Vector2 Position { get; set; }

        public List<Connection> Connections { get; }
            = new List<Connection>();
    }
}
