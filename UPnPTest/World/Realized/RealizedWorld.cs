using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PixelEngine;
using TileBasedSurvivalGame.World.Abstract;

namespace TileBasedSurvivalGame.World.Realized {
    class RealizedWorld {
        QuadTree<TerrainTile, TerrainTile.TerrainTilePositioner> Terrain { get; }

        public bool TerrainExistsAt(Location location) {
            return Terrain.GetWithinRect((location.X, location.Y), (location.X, location.Y)).Count() > 0;
        }

        public RealizedWorld(int size) {
            Terrain = new QuadTree<TerrainTile, TerrainTile.TerrainTilePositioner>(new AABB((-size, -size), (size, size)));
        }
    }

    class TerrainTile {
        public class TerrainTilePositioner
            : IPositioner<TerrainTile> {
            public Vector2 GetPosition(TerrainTile positioned) {
                return new Vector2(positioned.Location.X, positioned.Location.Y);
            }
        }
        public Location Location;
    }
}
