using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World.Abstract {
    class AbstractWorld {
        public QuadTree<WorldNode, WorldNodePositioner> Nodes { get; }
        public QuadTree<Entity, EntityPositioner> Entities { get; }

        public float MaxNodeEffectRadius { get; } = 255;

        //// get all nodes which might be affecting a given point, based on the max node effect radius
        public IEnumerable<WorldNode> NodesPossiblyAffectingPoint(Vector2 point) {
            foreach(WorldNode node in Nodes.GetWithinRadius(point, MaxNodeEffectRadius)) {
                yield return node;
            }
        }

        public AbstractWorld(int size) {
            Nodes = new QuadTree<WorldNode, WorldNodePositioner>(new AABB((-size, -size), (size, size)));
            Entities = new QuadTree<Entity, EntityPositioner>(Nodes.Bounds);

            // for now, add basic world origin
            Nodes.Add(new WorldNode() {
                Position = (0, 0),
                PositionLocked = true,
                Type = WorldNodeType.PossibleSpawnLocation,
                EffectRadius = MaxNodeEffectRadius,
                EffectFalloff = 0.5f
            });
        }
    }
}
