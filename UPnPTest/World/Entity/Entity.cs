using System.Collections.Generic;

namespace TileBasedSurvivalGame.World {
    class Entity {
        public Dictionary<string, bool> EntityFlags { get; set; } = new Dictionary<string, bool>();
        public Location WorldLocation { get; set; }
        public EntityController Controller { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public Entity(Location worldLocation) {
            WorldLocation = worldLocation;
            Width = 1;
            Height = 1;
        }

        public bool CollidesWith(Entity other) {
            if (other == null) return false;
            return WorldLocation.X + Width > other.WorldLocation.X
                && WorldLocation.Y + Height > other.WorldLocation.Y
                && WorldLocation.X < other.WorldLocation.X + other.Width
                && WorldLocation.Y < other.WorldLocation.Y + other.Height;
        }
    }
}
