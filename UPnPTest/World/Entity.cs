using System.Collections.Generic;

namespace TileBasedSurvivalGame.World {
    class Entity {
        public Location WorldLocation { get; set; }
        public List<Box> Colliders { get; }
        = new List<Box>();

        public Entity(Location worldLocation) {
            WorldLocation = worldLocation;
            Colliders.Add(new Box(1));
        }

        public bool CollidesWith(Entity other) {
            if(other == null) return false;
            return Box.AnyIntersect(Colliders, other.Colliders);
        }
    }
}
