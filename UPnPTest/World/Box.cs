using System.Collections.Generic;

namespace TileBasedSurvivalGame.World {
    //// a rectangular prism
    struct Box {
        public Location BottomNorthWestCorner {
            get => _bnwCorner;
            set {
                if (value.X > TopSouthEastCorner.X || value.Y > TopSouthEastCorner.Y || value.Z > TopSouthEastCorner.Z) {
                    Location temp = TopSouthEastCorner;
                    TopSouthEastCorner = value;
                    _bnwCorner = temp;
                }
                else {
                    _bnwCorner = value;
                }
            }
        }
        public Location TopSouthEastCorner {
            get => _tseCorner;
            set {
                if (value.X < BottomNorthWestCorner.X || value.Y < BottomNorthWestCorner.Y || value.Z < BottomNorthWestCorner.Z) {
                    Location temp = BottomNorthWestCorner;
                    BottomNorthWestCorner = value;
                    _tseCorner = temp;
                }
                else {
                    _tseCorner = value;
                }
            }
        }
        Location _bnwCorner, _tseCorner;

        public int West { get => BottomNorthWestCorner.X; set { BottomNorthWestCorner = BottomNorthWestCorner + Location.West * value; } }
        public int East { get => TopSouthEastCorner.X; set { TopSouthEastCorner = TopSouthEastCorner + Location.East * value; } }
        public int North { get => BottomNorthWestCorner.Y; set { BottomNorthWestCorner = BottomNorthWestCorner + Location.North * value; } }
        public int South { get => TopSouthEastCorner.Y; set { TopSouthEastCorner = TopSouthEastCorner + Location.South * value; } }
        public int Bottom { get => BottomNorthWestCorner.Z; set { BottomNorthWestCorner = BottomNorthWestCorner + Location.Down * value; } }
        public int Top { get => TopSouthEastCorner.Z; set { TopSouthEastCorner = TopSouthEastCorner + Location.Up * value; } }
    
        //// construct a square box
        public Box(int dimensions) {
            _bnwCorner = Location.Zero;
            _tseCorner = Location.One * dimensions;
        }
        public Box(Location origin, int westEast, int northSouth, int upDown) {
            _bnwCorner = origin;
            _tseCorner = origin + new Location(westEast, northSouth, upDown);
        }

        public bool Intersects(Box other) {
            return East > other.West
                && North > other.South
                && Top > other.Bottom
                && West < other.East
                && South < other.North
                && Bottom < other.Top;
        }
        public static bool AnyIntersect(IEnumerable<Box> a, IEnumerable<Box> b) {
            foreach (Box boxA in a) {
                foreach(Box boxB in b) {
                    if (boxA.Intersects(boxB)) {
                        return true;
                    }
                }
            }
            return true;
        }
    }
}
