using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelEngine;

using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Rendering {
    class Camera {
        public int RenderingHeight { get; set; } = 10;
        public bool DrawGrid { get; set; } = true;

        public void Render(Game context, TiledWorld world, Location location) {
            int screenTileWidth = context.ScreenWidth / TileRenderingHandler.TileSize;
            int screenTileHeight = context.ScreenHeight / TileRenderingHandler.TileSize;

            int ts = TileRenderingHandler.TileSize;

            // clear screen
            context.Clear(Pixel.Presets.Black);
            for (int x = 0; x < screenTileWidth; x++) {
                for (int y = 0; y < screenTileHeight; y++) {
                    Location twoDimTileLoc = Location.ToTile(location + new Location(x, y, 0));

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

                    for (int z = 0; z < RenderingHeight; z++) {
                        Location currentLocation = location + new Location(x, y, z);

                        Tile tile = world.GetTile(Location.WorldToChunk(currentLocation), Location.ToTile(currentLocation));
                        if (tile == null) continue;
                        if (TileTypeHandler.Invisible(tile.Type)) continue;

                        TileRenderingHandler.TileRenderingInfo renderingInfo = TileRenderingHandler.GetRenderingInfo(tile.Type);
                        if (renderingInfo == null) continue;

                        context.DrawRect(new Point(x * ts, y * ts), ts, ts, renderingInfo.InsideColour);
                        context.FillRect(new Point(x * ts, y * ts), ts, ts, renderingInfo.InsideColour);
                    }
                }
            }
        }
    }
}
