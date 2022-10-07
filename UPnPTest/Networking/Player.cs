//// = documentation
// = per-step working comments

using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Networking {
    class Player {
        public string Name { get; set; }
            = ReservedWords.Unset;
        public bool HasName => Name != ReservedWords.Unset;

        public int ID { get; set; }
        //// whether this player represents a local player, or a player over the network
        public bool Remote { get; set; }

        public Entity Entity { get; set; }

        public Player() { }
        public Player(int id, bool remote) {
            ID = id;
            Remote = remote;
        }
    }
}
