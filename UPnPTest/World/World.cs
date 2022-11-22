using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.World {
    class World {
        #region entity stuff

        public QuadTree<Entity, EntityPositioner> Entities { get; }

        public void Spawn(Entity entity) { }

        public bool HasEntityWithFlag(string flag, AABB region = null) {
            if(region == null) {
                region = Entities.Bounds;
            }
            foreach (Entity entity in Entities.GetWithinRect(region.TopLeft, region.BottomRight)) {
                return entity.EntityFlags.ContainsKey(flag)
                    && entity.EntityFlags[flag];
            }
            return false;
        }
        public IEnumerable<Entity> GetEntitiesWithFlag(string flag, AABB region = null) {
            if (region == null) {
                region = Entities.Bounds;
            }
            foreach (Entity entity in Entities.GetWithinRect(region.TopLeft, region.BottomRight)) {
                if (entity.EntityFlags.ContainsKey(flag) && entity.EntityFlags[flag]) {
                    yield return entity;
                }
            }
        }

        #endregion entity stuff

        public void SetTile(Location location, Tile tile) { }

        public void Tick(Engine context) { }

        public World(int size) {
            Entities = new QuadTree<Entity, EntityPositioner>(new AABB((-size, -size), (size, size)));
        }
    }

    class EntityPositioner : IPositioner<Entity> {
        public Vector2 GetPosition(Entity positioned) {
            return (positioned.WorldLocation.X, positioned.WorldLocation.Y);
        }
    }
}
