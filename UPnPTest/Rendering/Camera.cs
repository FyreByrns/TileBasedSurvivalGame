using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PixelEngine;

using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Rendering {
    class Camera {
        public TiledWorld World { get; }

        public int RenderingHeight { get; set; } = 10;
        public bool DrawGrid { get; set; } = false;

        // caching things
        Location _lastLocation;
        bool _changesSinceLastFrame;
        Dictionary<Location, Sprite> CachedChunkGraphics { get; }
        = new Dictionary<Location, Sprite>();
        public void Cache(Location chunk, Sprite sprite) {
            CachedChunkGraphics[chunk] = sprite;
        }
        public Sprite GetCachedSprite(Location chunk) {
            if (Cached(chunk)) {
                return CachedChunkGraphics[chunk];
            }
            return new Sprite(1, 1);
        }
        public bool Cached(Location chunk) {
            return CachedChunkGraphics.ContainsKey(chunk);
        }
        public void InvalidateCache(Location chunk) {
            _changesSinceLastFrame = true;
            if (Cached(chunk)) {
                CachedChunkGraphics.Remove(chunk);
            }
        }

        public void Render(Game context, Location location) {
            if (location != _lastLocation || _changesSinceLastFrame) {
                int screenTileWidth = context.ScreenWidth / TileRenderingHandler.TileSize;
                int screenTileHeight = context.ScreenHeight / TileRenderingHandler.TileSize;

                int ts = TileRenderingHandler.TileSize;
                Location minChunk = Location.ToChunk(location);
                Location maxChunk = Location.ToChunk(location + new Location(screenTileWidth, screenTileHeight));

                // clear screen
                context.Clear(Pixel.Presets.Black);
                context.PixelMode = Pixel.Mode.Alpha;

                for (int xChunk = minChunk.X; xChunk <= maxChunk.X; xChunk++) {
                    for (int yChunk = minChunk.Y; yChunk <= maxChunk.Y; yChunk++) {
                        Location currentChunk = new Location(xChunk, yChunk);

                        Location currentChunkCorner = Location.ToWorld(currentChunk, Location.Zero);
                        int chunkOffsetX = -(currentChunkCorner.X * ts - location.X * ts);
                        int chunkOffsetY = -(currentChunkCorner.Y * ts - location.Y * ts);

                        if (Cached(currentChunk)) {
                            // if the current chunk is cached, just draw cached image
                            context.DrawSprite(new Point(-chunkOffsetX, -chunkOffsetY), GetCachedSprite(currentChunk));
                        }
                        else {
                            // otherwise, draw and cache a new image
                            Sprite newChunkImage = new Sprite(Chunk.Size * ts, Chunk.Size * ts);
                            Sprite oldDrawTarget = context.DrawTarget;
                            context.DrawTarget = newChunkImage;

                            for (int x = 0; x < Chunk.Size; x++) {
                                for (int y = 0; y < Chunk.Size; y++) {
                                    Location twoDimTileLoc = new Location(x, y);

                                    // draw grid
                                    if (DrawGrid) {
                                        context.Draw(x * ts, y * ts, Pixel.Presets.DarkMagenta);
                                        // x chunk borders
                                        if (twoDimTileLoc.X == 0 || twoDimTileLoc.X == Chunk.Size - 1) {
                                            context.Draw(x * ts + ts / 2, y * ts + ts / 2, Pixel.Presets.Magenta);
                                        }
                                        // y chunk borders
                                        if (twoDimTileLoc.Y == 0 || twoDimTileLoc.Y == Chunk.Size - 1) {
                                            context.Draw(x * ts + ts / 2, y * ts + ts / 2, Pixel.Presets.Magenta);
                                        }
                                    }

                                    Location currentLocation = new Location(x, y);

                                    Tile tile = World.GetTile(currentChunk, currentLocation);
                                    if (tile == null) continue;
                                    if (TileTypeHandler.Invisible(tile.Type)) continue;

                                    TileRenderingHandler.TileRenderingInfo renderingInfo = TileRenderingHandler.GetRenderingInfo(tile.Type);
                                    if (renderingInfo == null) continue;

                                    context.DrawRect(new Point(x * ts, y * ts), ts, ts, renderingInfo.InsideColour);
                                    context.FillRect(new Point(x * ts, y * ts), ts, ts, renderingInfo.InsideColour);
                                }
                            }

                            context.DrawTarget = oldDrawTarget;
                            Cache(currentChunk, newChunkImage);
                            context.DrawSprite(new Point(-chunkOffsetX, -chunkOffsetY), GetCachedSprite(currentChunk));
                        }
                    }
                }
            }

            context.PixelMode = Pixel.Mode.Normal;
            _lastLocation = location;
            _changesSinceLastFrame = false;
        }

        public Camera(TiledWorld world) {
            World = world;
            World.WorldChange += WorldChanged;
        }

        private void WorldChanged(Location chunkLoc, Location tileLoc, Tile tile, bool fromServer) {
            InvalidateCache(chunkLoc);
        }
    }
}
