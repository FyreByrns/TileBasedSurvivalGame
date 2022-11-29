using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PixelEngine;
using TileBasedSurvivalGame.World.Abstract;

namespace TileBasedSurvivalGame.World.Realized {
    class RealizedWorld {
        QuadTree<TerrainTile, TerrainTile.TerrainTilePositioner> Terrain { get; }

        public bool TerrainExistsAt(Location location) {
            return Terrain.GetWithinRect((location.X, location.Y), (location.X, location.Y)).Count() > 0;
        }
        public TerrainTile GetTerrainAt(Location location) {
            return Terrain.GetWithinRect((location.X, location.Y), (location.X, location.Y)).First() ?? default;
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
            Terrain = new QuadTree<TerrainTile, TerrainTile.TerrainTilePositioner>(new AABB((-size, -size), (size, size)));
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
    }
}
