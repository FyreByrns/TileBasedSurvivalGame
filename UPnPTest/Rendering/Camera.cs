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

            for (int x = 0; x < screenTileWidth; x++) {
                for (int y = 0; y < screenTileHeight; y++) {
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
