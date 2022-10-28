using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    class AbstractWorld {
        public QuadTree Nodes { get; }
        = new QuadTree(new AABB((0, 0), (1, 1)));
    }
}
