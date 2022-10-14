using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PixelEngine;

using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Rendering {
    class Camera {
        public int RenderingHeight { get; set; } = 10;

        // caching things
        Location _lastLocation;
        bool _changesSinceLastFrame;
        public void InvalidateCache() {
            _changesSinceLastFrame = true;
        }

        public void Render(Engine context, TiledWorld world, Location location) {
            int ts = TileRenderingHandler.TileSize;

            if (location != _lastLocation || _changesSinceLastFrame) {
                int screenTileWidth = context.ScreenWidth / TileRenderingHandler.TileSize;
                int screenTileHeight = context.ScreenHeight / TileRenderingHandler.TileSize;

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

                        Chunk chunk = world.GetChunk(currentChunk);

                        if (chunk.Graphics == null) {
                            // if there are no cached graphics, regenerate them
                            chunk.RegenerateGraphics(context);
                        }
                        context.DrawSprite(new Point(-chunkOffsetX, -chunkOffsetY), chunk.Graphics);
                    }
                }
            }

            lock (world.Entities) {
                foreach (Entity entity in world.Entities) {
                    for (int bodyX = 0; bodyX < entity.Width; bodyX++) {
                        for (int bodyY = 0; bodyY < entity.Height; bodyY++) {
                            context.DrawRect(new Point((bodyX + entity.WorldLocation.X - location.X) * ts, (bodyY + entity.WorldLocation.Y - location.Y) * ts), ts, ts, Pixel.Presets.Red);
                        }
                    }
                }
            }

            context.PixelMode = Pixel.Mode.Normal;
            _lastLocation = location;
            _changesSinceLastFrame = false;
        }

        public void WorldChanged(Location chunkLoc, Location tileLoc, Tile tile, bool fromServer) {
            InvalidateCache();
        }

        public void EntityMoved(Entity entity, Location worldFrom, Location worldTo) {
            InvalidateCache();
        }
    }
}
