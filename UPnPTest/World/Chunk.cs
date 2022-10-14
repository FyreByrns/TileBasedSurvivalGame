using TileMap = System.Collections.Generic.Dictionary<TileBasedSurvivalGame.World.Location, TileBasedSurvivalGame.World.Tile>;
using EntityList = System.Collections.Generic.HashSet<TileBasedSurvivalGame.World.Entity>;

using Sprite = PixelEngine.Sprite;
using Context = PixelEngine.Game;
using Pixel = PixelEngine.Pixel;
using Point = PixelEngine.Point;

namespace TileBasedSurvivalGame.World {
    class Chunk {
        public static bool DrawGrid { get; set; } = false;

        //// x and y dimensions
        public static int Size = 30;

        public bool Changed { get; private set; } = false;

        public TileMap Tiles { get; private set; }
        = new TileMap();

        public EntityList ChunkLocalEntities { get; }
        = new EntityList();

        public Sprite Graphics { get; private set; }

        public Tile GetTile(Location tileLocation) {
            if (Tiles.ContainsKey(tileLocation)) {
                return Tiles[tileLocation];
            }
            return null;
        }
        public void SetTile(Location tileLocation, Tile tile) {
            Tiles[tileLocation] = tile;

            // regenerate graphics
            Changed = true;
        }

        public void RegenerateGraphics(Engine context) {
            int ts = Rendering.TileRenderingHandler.TileSize;

            if (Graphics == null) {
                Graphics = new Sprite(ts * Size, ts * Size);
            }

            Sprite oldDrawTarget = context.DrawTarget;
            context.DrawTarget = Graphics;

            for (int x = 0; x < Size; x++) {
                for (int y = 0; y < Size; y++) {
                    Location twoDimTileLoc = new Location(x, y);

                    // draw grid
                    if (DrawGrid) {
                        context.Draw(x * ts, y * ts, Pixel.Presets.DarkMagenta);
                        // x chunk borders
                        if (twoDimTileLoc.X == 0 || twoDimTileLoc.X == Size - 1) {
                            context.Draw(x * ts + ts / 2, y * ts + ts / 2, Pixel.Presets.Magenta);
                        }
                        // y chunk borders
                        if (twoDimTileLoc.Y == 0 || twoDimTileLoc.Y == Size - 1) {
                            context.Draw(x * ts + ts / 2, y * ts + ts / 2, Pixel.Presets.Magenta);
                        }
                    }

                    Location currentLocation = new Location(x, y);

                    Tile tile = GetTile(currentLocation);
                    if (tile == null) continue;
                    if (TileTypeHandler.Invisible(tile.Type)) continue;

                    Rendering.TileRenderingHandler.TileRenderingInfo renderingInfo = Rendering.TileRenderingHandler.GetRenderingInfo(tile.Type);
                    if (renderingInfo == null) continue;

                    context.DrawRect(new Point(x * ts, y * ts), ts, ts, renderingInfo.InsideColour);
                    context.FillRect(new Point(x * ts, y * ts), ts, ts, renderingInfo.InsideColour);
                }
            }

            context.DrawTarget = oldDrawTarget;
        }
    }
}
