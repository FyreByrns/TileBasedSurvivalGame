using ChunkMap = System.Collections.Generic.Dictionary<TileBasedSurvivalGame.World.Location, TileBasedSurvivalGame.World.Chunk>;

namespace TileBasedSurvivalGame.World {
    //// chunked tiled world, arbitrary height
    class TiledWorld {
        public delegate void WorldChangeEventHandler(Location chunkLoc, Location tileLoc, Tile tile, bool fromServer);
        public event WorldChangeEventHandler WorldChange;

        public ChunkMap Chunks { get; private set; }
        = new ChunkMap();

        public Location TileLocationToChunkLocation(Location tileLocation) {
            int x = tileLocation.X % Chunk.Size;
            int y = tileLocation.Y % Chunk.Size;
            int z = tileLocation.Z % Chunk.Size;
            return new Location(x, y, z, tileLocation.Dimension);
        }

        public Chunk GetChunk(Location chunkLocation) {
            if (HasChunk(chunkLocation)) {
                return Chunks[chunkLocation];
            }
            else return NewChunk(chunkLocation);
        }
        public void SetChunk(Location chunkLocation, Chunk chunk) {
            Chunks[chunkLocation] = chunk;
        }
        public Chunk NewChunk(Location chunkLocation) {
            // for now, just create and return a new chunk
            Chunk result = new Chunk();
            Chunks[chunkLocation] = result;
            return result;
            // later, generate a chunk
        }
        public bool HasChunk(Location chunkLocation) {
            return Chunks.ContainsKey(chunkLocation);
        }

        public Tile GetTile(Location chunkLoc, Location tileLoc) {
            if (HasChunk(chunkLoc)) {
                Chunk chunk = GetChunk(chunkLoc);
                Tile tile = chunk.GetTile(tileLoc);
                return tile;
            }
            return null;
        }
        public void SetTile(Location chunkLoc, Location tileLoc, Tile tile, bool silent = false, bool fromServer = false) {
            GetChunk(chunkLoc).SetTile(tileLoc, tile);
            if (!silent) {
                WorldChange?.Invoke(chunkLoc, tileLoc, tile, fromServer);
            }
        }
    }
}
