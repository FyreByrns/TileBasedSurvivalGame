namespace TileBasedSurvivalGame.World {
    struct Location {
        public static readonly Location Zero = new Location(0, 0);
        public static readonly Location One = new Location(1, 1);

        public static readonly Location West = new Location(-1, 0);
        public static readonly Location East = new Location(+1, 0);
        public static readonly Location North = new Location(0, -1);
        public static readonly Location South = new Location(0, +1);

        public int X { get; set; } //  -west | east+
        public int Y { get; set; } // -north | south+

        public int Dimension { get; set; }

        public Location(int x, int y) : this(x, y, 0) { }
        public Location(int x, int y, int dimension) {
            X = x;
            Y = y;
            Dimension = dimension;
        }


        public static Location operator *(Location a, int scale) {
            return new Location(a.X * scale, a.Y * scale);
        }
        public static Location operator /(Location a, int scale) {
            return new Location(a.X / scale, a.Y / scale);
        }
        public static Location operator +(Location a, Location b) {
            return new Location(a.X + b.X, a.Y + b.Y);
        }
        public static Location operator -(Location a, Location b) {
            return new Location(a.X - b.X, a.Y - b.Y);
        }
        public static bool operator ==(Location a, Location b) {
            return a.X == b.X
                && a.Y == b.Y
                && a.Dimension == b.Dimension;
        }
        public static bool operator !=(Location a, Location b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            if (obj is Location other) {
                return other == this;
            }
            return false;
        }
        //public override int GetHashCode() {
        //    return X.GetHashCode() ^ Y.GetHashCode();
        //}
    }
}
