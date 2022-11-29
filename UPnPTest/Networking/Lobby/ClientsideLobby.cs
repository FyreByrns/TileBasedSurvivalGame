using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TileBasedSurvivalGame.Networking.Messages;

namespace TileBasedSurvivalGame.Networking {
    partial class Lobby {
        public int ClientID { get; set; }
        public Dictionary<int, UserData> RemotePlayers { get; }
        = new Dictionary<int, UserData>();

        public World.World ClientWorld { get; set; }

        private void ClientMessageReceived(NetMessage message) {
            if (message is AllowConnection ac) {
                ClientID = ac.ClientID;
                Logger.Log($"server accepted connection! my ID is {ClientID}");
                NetHandler.SendToServer(new TextMessage("hi! I am the client."));

                // remove remote player with same ID
                foreach (int id in RemotePlayers.Keys.ToArray()) {
                    if (id == ClientID) {
                        RemotePlayers.Remove(id);
                    }
                }
            }

            if (message is TextMessage textMessage) {
                if (textMessage.OriginatingID == ClientID) {
                    Logger.Log("e");
                }
                else {
                    Logger.Log($"c rcv {textMessage.Text}");
                }
            }

            if (message is PlayerList playerList) {
                foreach (int id in playerList.IDs) {
                    if (id == ClientID) {
                        continue;
                    }

                    Logger.Log($"new connection: \t{id}");
                    RemotePlayers[id] = new UserData();
                    RemotePlayers[id].ID = id;
                }
            }
        }
    }

    class ClientsideLobbyState : LobbyState {
        List<Type> AlwaysExpecting { get; } = new List<Type>() {
            typeof(TextMessage),
            typeof(PlayerList),
        };

        public override bool ExpectingMessageOfType<T>() {
            // never expecting messages that aren't intended to be sent to the client
            if (!NetMessage.ExpectedOnClient<T>()) {
                return false;
            }

            // always expecting these types of messages
            if (AlwaysExpecting.Contains(typeof(T))){
                return true;
            }



            return false;
        }
    }
}
