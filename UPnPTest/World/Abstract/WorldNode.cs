using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    enum WorldNodeType {
        None,

        Coastline,
    }

    class WorldNodePositioner : IPositioner<WorldNode> {
        public Vector2 GetPosition(WorldNode node) {
            return node.Position;
        }
    }

    class WorldNode {
        public Vector2 Position { get; set; }
        public bool PositionLocked { get; set; }
        public WorldNodeType Type;
        public float EffectRadius;

        public HashSet<WorldNode> ConnectedNodes { get; private set; }

        public void Connect(WorldNode other, bool connectFromOther = true) {
            if (ConnectedNodes == null) {
                ConnectedNodes = new HashSet<WorldNode>();
            }

            // don't connect to self
            if (other == this) {
                return;
            }

            ConnectedNodes.Add(other);
            if (connectFromOther) {
                other.Connect(this, false);
            }
        }
        public void Disconnect(WorldNode other) {
            if (ConnectedNodes?.Contains(other) ?? false) {
                ConnectedNodes.Remove(other);
                other.Disconnect(this);
            }
        }
        public void DisconnectFromAll() {
            if (ConnectedNodes != null) {
                foreach (WorldNode node in ConnectedNodes.ToArray()) {
                    Disconnect(node);
                }
            }
        }
    }
}
