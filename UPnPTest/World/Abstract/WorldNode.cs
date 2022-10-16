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
        public bool PositionLocked { get; set; }
        public WorldNodeType Type;

        public Connection ConnectionToParent { get; set; }
        public List<Connection> Connections { get; }
            = new List<Connection>();

        public IEnumerable<WorldNode> GetAllChildren() {
            yield return this;
            foreach (Connection connection in Connections) {
                foreach (WorldNode node in connection?.B?.GetAllChildren()) {
                    yield return node;
                }
            }
        }
    }
}
