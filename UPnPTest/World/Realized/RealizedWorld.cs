using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PixelEngine;
using TileBasedSurvivalGame.World.Abstract;

namespace TileBasedSurvivalGame.World.Realized {
    class RealizedWorld {
        List<Chunk> Terrain = new List<Chunk> ();

        public bool TerrainExistsAt(Location location) {
            foreach (Chunk chunk in Terrain) {
                if (chunk.WithinBounds(location)) {
                    return chunk.TileExists(location);
                }
            }
            return false;
        }
        public TerrainTile GetTerrainAt(Location location) {
            foreach (Chunk chunk in Terrain) {
                if (chunk.WithinBounds(location)) {
                    return chunk.GetTile(location);
                }
            }
            return null;
        }

        //// generate a rectangle of terrain from an abstract world
        public void GenerateTerrainInRect(Engine context, AbstractWorld abstractWorld, AABB bounds) {
            int offsetX = (int)bounds.TopLeft.x;
            int offsetY = (int)bounds.TopLeft.y;
            int width = (int)bounds.Width;
            int height = (int)bounds.Height;

            // generation image, so lines etc can be created
            Sprite generationImage = new Sprite(width, height);
            Sprite oldTarget = context.DrawTarget;
            context.DrawTarget = generationImage;
            context.PixelMode = Pixel.Mode.Alpha;

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < width; y++) {
                    Vector2 pixelLoc = (x, y);
                    Vector2 worldPixelLoc = pixelLoc + (offsetX, offsetY);

                    foreach (WorldNode node in abstractWorld.NodesPossiblyAffectingPoint((x + offsetX, y + offsetY))) {
                        Vector2 offsetVector = worldPixelLoc - node.Position;
                        byte distance = (byte)context.Map(offsetVector.Length, 0, node.EffectRadius, 255, 0);

                        context.Draw(x, y, new Pixel((byte)(distance * node.EffectFalloff), 0, 0, (byte)(255f * 0.5f)));
                    }
                }
            }

            Sprite.Save(generationImage, "testGenImageStage1.png");

            context.PixelMode = Pixel.Mode.Normal;
            context.DrawTarget = oldTarget;
        }

        public RealizedWorld(int size) {
        }
    }

    class Chunk {
        public const int Width = 10;
        public const int Height = 10;

        // position of the chunk within the world
        public Location Location;
        public Dictionary<Location, TerrainTile> Tiles { get; } = new Dictionary<Location, TerrainTile>();

        public bool WithinBounds(Location worldLocation) {
            return worldLocation.X - Location.X < Width && worldLocation.Y - Location.Y < Height;
        }
        public Location ToLocal(Location worldLocation) {
            return new Location(worldLocation.X - Location.X, worldLocation.Y - Location.Y);
        }
        public Location ToGlobal(Location localLocation) {
            return new Location(Location.X + localLocation.X, Location.Y + localLocation.Y);
        }

        public bool TileExists(Location worldLocation) {
            if (WithinBounds(worldLocation)) {
                return Tiles.ContainsKey(ToLocal(worldLocation));
            }
            return false;
        }
        public void SetTile(Location worldLocation, TerrainTile tile) {
            if (WithinBounds(worldLocation)) {
                Tiles[ToLocal(worldLocation)] = tile;
            }
        }
        public TerrainTile GetTile(Location worldLocation) {
            if (TileExists(worldLocation)) {
                return Tiles[ToLocal(worldLocation)];
            }
            return null;
        }
        public void RemoveTile(Location worldLocation, TerrainTile tile) {
            if (WithinBounds(worldLocation) && Tiles.ContainsKey(ToLocal(worldLocation))) {
                Tiles.Remove(ToLocal(worldLocation));
            }
        }
    }

    class TerrainTile {
        public class TerrainTilePositioner
            : IPositioner<TerrainTile> {
            public Vector2 GetPosition(TerrainTile positioned) {
                return new Vector2(positioned.Location.X, positioned.Location.Y);
            }
        }
        public Location Location;

        public HashSet<Entity> Entities { get; }
         = new HashSet<Entity>();
    }
}
