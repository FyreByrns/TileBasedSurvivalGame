using TileMap = System.Collections.Generic.Dictionary<TileBasedSurvivalGame.World.Location, TileBasedSurvivalGame.World.Tile>;
using EntityList = System.Collections.Generic.HashSet<TileBasedSurvivalGame.World.Entity>;

namespace TileBasedSurvivalGame.World {
    class Chunk {
        //// x and y dimensions
        public static int Size = 30;

        public TileMap Tiles { get; private set; }
        = new TileMap();

        public EntityList ChunkLocalEntities { get; }
        = new EntityList();

        public Tile GetTile(Location tileLocation) {
            if (Tiles.ContainsKey(tileLocation)) {
                return Tiles[tileLocation];
            }
            return null;
        }
        public void SetTile(Location tileLocation, Tile tile) {
            Tiles[tileLocation] = tile;
        }
    }
}
