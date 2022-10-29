using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    class AbstractWorld {
        public QuadTree<WorldNode, WorldNodePositioner> Nodes { get; }
        
        public AbstractWorld(int size) {
            Nodes = new QuadTree<WorldNode, WorldNodePositioner>(new AABB((-size, -size), (size, size)));
        }
    }
}
