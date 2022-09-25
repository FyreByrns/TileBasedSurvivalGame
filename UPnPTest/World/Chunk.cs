using TileMap = System.Collections.Generic.Dictionary<TileBasedSurvivalGame.World.Location, TileBasedSurvivalGame.World.Tile>;

namespace TileBasedSurvivalGame.World {
    class Chunk {
        //// x, y, and z dimensions
        public static int Size = 30;

        public TileMap Tiles { get; private set; }
        = new TileMap();

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
