using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TileBasedSurvivalGame.World.Abstract;
using TileBasedSurvivalGame.World.Realized;

namespace TileBasedSurvivalGame.World {
    class World {
        public AbstractWorld AbstractWorld { get; }
        public RealizedWorld RealizedWorld { get; }

        public QuadTree<Entity, EntityPositioner> Entities => AbstractWorld.Entities;

        #region ===== Entity Stuff =====
        public void Spawn(Entity entity) { }
        public bool HasEntityWithFlag(string flag, AABB region = null) {
            if (region == null) {
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
        #endregion
        #region =====  Tile Stuff  =====
        public void SetTile(Location location, Tile tile) { }
        public TerrainTile GetTile(Location location) {
            if (RealizedWorld.TerrainExistsAt(location)) {
                return RealizedWorld.GetTerrainAt(location);
            }
            return default;
        }
        #endregion


        public void Tick(Engine context) { }

        public World(int size) {
            AbstractWorld = new AbstractWorld(size);
            RealizedWorld = new RealizedWorld(size);
        }
    }

    class EntityPositioner : IPositioner<Entity> {
        public Vector2 GetPosition(Entity positioned) {
            return (positioned.WorldLocation.X, positioned.WorldLocation.Y);
        }
    }
}
