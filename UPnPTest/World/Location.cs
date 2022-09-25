namespace TileBasedSurvivalGame.World {
    struct Location {
        public static readonly Location Zero = new Location(0, 0, 0);
        public static readonly Location One = new Location(1, 1, 1);

        public static readonly Location West = new Location(-1, 0, 0);
        public static readonly Location East = new Location(+1, 0, 0);
        public static readonly Location North = new Location(0, -1, 0);
        public static readonly Location South = new Location(0, +1, 0);
        public static readonly Location Down = new Location(0, 0, -1);
        public static readonly Location Up = new Location(0, 0, +1);

        public int X { get; set; } //  -west | east+
        public int Y { get; set; } // -north | south+
        public int Z { get; set; } //  -down | up+

        public int Dimension { get; set; }

        public Location(int x, int y, int z) : this(x, y, z, 0) { }
        public Location(int x, int y, int z, int dimension) {
            X = x;
            Y = y;
            Z = z;
            Dimension = dimension;
        }

        //// find what chunk a global location is in
        public static Location WorldToChunk(Location world) {
            return world / Chunk.Size;
        }
        //// find what tile a global location is in a chunk
        public static Location WorldToTile(Location world) {
            return new Location(world.X % Chunk.Size, world.Y % Chunk.Size, world.Z % Chunk.Size);
        }
        //// find global location from a chunk and tile location
        public static Location ToWorld(Location chunk, Location tile) {
            return chunk * Chunk.Size + tile;
        }

        public static Location operator *(Location a, int scale) {
            return new Location(a.X * scale, a.Y * scale, a.Z * scale, a.Dimension);
        }
        public static Location operator /(Location a, int scale) {
            return new Location(a.X / scale, a.Y / scale, a.Z / scale);
        }
        public static Location operator +(Location a, Location b) {
            return new Location(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.Dimension);
        }
        public static Location operator -(Location a, Location b) {
            return new Location(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.Dimension);
        }
        public static bool operator ==(Location a, Location b) {
            return a.X == b.X
                && a.Y == b.Y
                && a.Z == b.Z
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
        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
    }
}
