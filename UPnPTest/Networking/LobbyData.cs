using System.Collections.Generic;

//// = documentation
// = per-step working comments

using PlayerID = System.Int32;

namespace TileBasedSurvivalGame.Networking {
    class LobbyData {
        public Dictionary<PlayerID, Player> Players { get; }
            = new Dictionary<PlayerID, Player>();

        public void AddPlayer(PlayerID playerID, Player player) {
            Players[playerID] = player;
        }
        public Player GetPlayer(PlayerID playerID) {
            if (Players.ContainsKey(playerID)) {
                return Players[playerID];
            }
            return null;
        }
    }
}
