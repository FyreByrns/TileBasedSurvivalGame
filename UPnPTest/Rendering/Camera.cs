using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelEngine;

using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Rendering {
    class Camera {
        public int RenderingHeight { get; set; } = 10;

        public void Render(Game context, TiledWorld world, Location location) {
            int screenTileWidth = context.ScreenWidth / TileRenderingHandler.TileSize;
            int screenTileHeight = context.ScreenHeight / TileRenderingHandler.TileSize;

            // clear screen
            context.Clear(Pixel.Presets.Black);
            for (int x = 0; x < screenTileWidth; x++) {
                for (int y = 0; y < screenTileHeight; y++) {
                    // draw grid
                    context.Draw(
                        x * TileRenderingHandler.TileSize,
                        y * TileRenderingHandler.TileSize,
                        Pixel.Presets.DarkMagenta);
                    // x chunk borders
                    if (x % Chunk.Size == 0 || x % Chunk.Size == Chunk.Size - 1) {
                        context.Draw(
                            x * TileRenderingHandler.TileSize + TileRenderingHandler.TileSize / 2,
                            y * TileRenderingHandler.TileSize + TileRenderingHandler.TileSize / 2,
                            Pixel.Presets.Magenta);
                    }
                    // y chunk borders
                    if (y % Chunk.Size == 0 || y % Chunk.Size == Chunk.Size - 1) {
                        context.Draw(
                            x * TileRenderingHandler.TileSize + TileRenderingHandler.TileSize / 2,
                            y * TileRenderingHandler.TileSize + TileRenderingHandler.TileSize / 2,
                            Pixel.Presets.Magenta);
                    }

                    for (int z = 0; z < RenderingHeight; z++) {
                        Location currentLocation = location + new Location(x, y, z);

                        Tile tile = world.GetTile(Location.WorldToChunk(currentLocation), Location.WorldToTile(currentLocation));
                        if (tile == null) continue;
                        if (TileTypeHandler.Invisible(tile.Type)) continue;

                        TileRenderingHandler.TileRenderingInfo renderingInfo = TileRenderingHandler.GetRenderingInfo(tile.Type);
                        if(renderingInfo == null) continue;

                        context.FillRect(
                            new Point(
                                x * TileRenderingHandler.TileSize,
                                y * TileRenderingHandler.TileSize),
                                TileRenderingHandler.TileSize,
                                TileRenderingHandler.TileSize,
                                renderingInfo.InsideColour);
                        context.DrawRect(
                            new Point(
                                x * TileRenderingHandler.TileSize,
                                y * TileRenderingHandler.TileSize),
                                TileRenderingHandler.TileSize,
                                TileRenderingHandler.TileSize,
                                renderingInfo.InsideColour);
                    }
                }
            }
        }
    }
}
