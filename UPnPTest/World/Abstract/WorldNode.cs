﻿using System;
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

        /// <summary>
        /// Get all connected nodes and all their connected nodes.
        /// </summary>
        public IEnumerable<WorldNode> GetAllRelatives() {
            if (ConnectedNodes != null) {
                foreach (WorldNode node in ConnectedNodes) {
                    yield return node;
                    foreach (WorldNode nodeConnection in node.GetAllRelatives()) {
                        yield return nodeConnection;
                    }
                }
            }
        }

        public void Connect(WorldNode other, bool connectFromOther = true) {
            if (ConnectedNodes == null) {
                ConnectedNodes = new HashSet<WorldNode>();
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
