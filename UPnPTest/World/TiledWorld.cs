using ChunkMap = System.Collections.Generic.Dictionary<TileBasedSurvivalGame.World.Location, TileBasedSurvivalGame.World.Chunk>;
using EntityList = System.Collections.Generic.HashSet<TileBasedSurvivalGame.World.Entity>;

namespace TileBasedSurvivalGame.World {
    //// chunked tiled world, arbitrary height
    class TiledWorld : ITickable {
        public delegate void WorldChangeEventHandler(Location chunkLoc, Location tileLoc, Tile tile, bool fromServer);
        public event WorldChangeEventHandler WorldChange;
        public delegate void EntityMovedEventHandler(Entity entity, Location worldFrom, Location worldTo);
        public event EntityMovedEventHandler EntityMoved;

        public ChunkMap Chunks { get; private set; }
        = new ChunkMap();

        public EntityList Entities { get; }
        = new EntityList();

        public Location TileLocationToChunkLocation(Location tileLocation) {
            int x = tileLocation.X % Chunk.Size;
            int y = tileLocation.Y % Chunk.Size;
            return new Location(x, y, tileLocation.Dimension);
        }

        public System.Collections.Generic.IEnumerable<Chunk> GetChunks(params Location[] chunkLocations) {
            foreach (Location loc in chunkLocations) {
                yield return GetChunk(loc);
            }
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

        // update chunk local entity lists
        // this approach IGNORES ENTITIES LARGER THAN A CHUNK but that should never be a problem
        // .. please let it never be a problem
        // .. if it is, the fix is relatively simple
        // .. I just don't want to deal with it
        void UpdateChunkInhabitantList(Entity entity) {
            foreach (Chunk previousChunk in entity.InhabitedChunks) {
                previousChunk.ChunkLocalEntities.Remove(entity);
            }

            entity.InhabitedChunks.Clear();
            // just using the corners should be fine (see note above)
            Location northWestCornerChunk = Location.ToChunk(entity.WorldLocation);
            Location northEastCornerChunk = Location.ToChunk(entity.WorldLocation + new Location(entity.Width, 0));
            Location southWestCornerChunk = Location.ToChunk(entity.WorldLocation + new Location(0, entity.Height));
            Location southEastCornerChunk = Location.ToChunk(entity.WorldLocation + new Location(entity.Width, entity.Height));
            foreach (Chunk inhabitedChunk in GetChunks(northWestCornerChunk, northEastCornerChunk, southWestCornerChunk, southEastCornerChunk)) {
                inhabitedChunk.ChunkLocalEntities.Add(entity);
                entity.InhabitedChunks.Add(inhabitedChunk);
            }
        }

        public void Tick(Engine context) {
            // tick entities
            TickEntities();
            // tick world
            TickWorld();
        }

        void TickEntities() {
            foreach (Entity entity in Entities) {
                // update controller
                entity.Controller.Update(this);

                // resolve any desired movement
                // current method of movement will need to be reconsidered should
                // .. knockback ever be desired
                // todo: move movement resolution to own method

                // store old location
                Location oldLocation = entity.WorldLocation;

                // check if the new location overlaps anything solid
                bool moveSuccess = true;
                for (int newBodyX = 0; newBodyX < entity.Width; newBodyX++) {
                    for (int newBodyY = 0; newBodyY < entity.Height; newBodyY++) {
                        Location newBodyLocation = new Location(newBodyX, newBodyY) + entity.Controller.DesiredLocation;
                        Location newBodyChunk = Location.ToChunk(newBodyLocation);
                        Location newBodyTile = Location.ToTile(newBodyLocation);

                        Tile tileAt = GetTile(newBodyChunk, newBodyTile);
                        if (tileAt != null) {
                            if (TileTypeHandler.Solid(tileAt.Type)) {
                                moveSuccess = false;
                                // todo: early exit
                            }
                        }
                    }
                }

                if (moveSuccess) {
                    entity.WorldLocation = entity.Controller.DesiredLocation;
                    EntityMoved?.Invoke(entity, oldLocation, entity.WorldLocation);
                    // todo: propagate footstep event for the movement
                }

                // while we're looping through entities, might as well do this too
                UpdateChunkInhabitantList(entity);
            }
        }
        void TickWorld() { }
    }
}
