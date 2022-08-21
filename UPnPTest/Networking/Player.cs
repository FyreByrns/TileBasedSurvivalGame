//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame.Networking {
    class Player {
        public string Name { get; private set; }
            = ReservedWords.Unset;
        public bool HasName { get; private set; }
    }
}
