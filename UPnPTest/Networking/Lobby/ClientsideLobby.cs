using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TileBasedSurvivalGame.Networking.Messages;

namespace TileBasedSurvivalGame.Networking {
    partial class Lobby {
        public ClientsideLobbyState ClientsideLobbyState { get; }

        public UserData ClientData { get; set; }
        public Dictionary<int, UserData> RemotePlayers { get; }
        = new Dictionary<int, UserData>();

        public World.World ClientWorld { get; set; }

        public UserData GetRemotePlayer(int id) {
            if (RemotePlayers.ContainsKey(id)) {
                return RemotePlayers[id];
            }
            return new UserData();
        }
        public void AddRemotePlayer(int id) {
            RemotePlayers[id] = new UserData();
            RemotePlayers[id].ID = id;
        }
        public void RemoveRemotePlayer(int id) {
            RemotePlayers.Remove(id);
        }

        private void ClientMessageReceived(NetMessage message) {
            ClientsideLobbyState.HandleMessage(message, this);
        }
    }

    class ClientsideLobbyState : LobbyState {
        public override Dictionary<Type, NetMessageHandler> MessageHandlers => new Dictionary<Type, NetMessageHandler>{
            { typeof(AllowConnection), (m, l, s) => {
                var ac = (AllowConnection)m;
                l.ClientData.ID = ac.ClientID;
                Logger.Log($"server accepted connection! my ID is {l.ClientData.ID}");
                NetHandler.SendToServer(new TextMessage("hi! I am the client."));

                // remove remote player with same ID
                foreach (int id in l.RemotePlayers.Keys.ToArray()) {
                    if (id == l.ClientData.ID) {
                        l.RemoveRemotePlayer(id);
                    }
                }
            } },
            { typeof(TextMessage), (m, l, s) => { 
                var tm = (TextMessage)m;
                if(tm.OriginatingID == l.ClientData.ID) {
                    Logger.Log("server echoed sent message");
                }
                else {
                    Logger.Log($"{tm.OriginatingID}: {tm.Text}");
                }
            } },
            { typeof(PlayerList), (m, l, s) => { 
                var pl = (PlayerList)m;
                foreach (int id in pl.IDs) {
                    if (id == l.ClientData.ID) {
                        continue;
                    }

                    Logger.Log($"new connection: \t{id}");
                    l.AddRemotePlayer(id);
                }
            } },
            { typeof(DisallowName), (m, l, s) => {
                // todo: inform of name not being allowed and request a new name
                // for now: just send ID as a string
                NetHandler.SendToServer(new SetName(l.ClientData.ID.ToString()));
            } },
            { typeof(NamesList), (m, l, s) => {
                var nl = (NamesList)m;
                // read all names 
                for(int i = 0; i < nl.Names.Length; i++) {
                    int id = nl.ClientIDs[i];
                    string name = nl.Names[i];
                    l.GetRemotePlayer(id).Name = name;

                    Logger.Log($"{id} is called {name}");

                    if(id == l.ClientData.ID) {
                        l.ClientData.Name = name;
                    }
                }
            } },
        };

        public override bool ExpectingMessageOfType(Type t) {
            // never expecting messages that aren't intended to be sent to the client
            if (!NetMessage.ExpectedOnClient(t)) {
                return false;
            }
            return true;
        }
    }
}
