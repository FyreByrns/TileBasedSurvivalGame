using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IPEndPoint = System.Net.IPEndPoint;
using TileBasedSurvivalGame.Networking.Messages;
using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Networking {
    partial class Lobby {
        public ServersideLobbyState ServersideLobbyState { get; }

        public Dictionary<IPEndPoint, UserData> ConnectedClients { get; }
        = new Dictionary<IPEndPoint, UserData>();

        public World.World ServerWorld { get; set; }

        public int GetConnectedClientID(IPEndPoint endpoint) {
            if (ConnectedClients.ContainsKey(endpoint)) {
                return ConnectedClients[endpoint].ID;
            }
            return -1;
        }
        public bool AlreadyConnected(IPEndPoint endpoint) {
            return ConnectedClients.ContainsKey(endpoint);
        }
        public void RegisterNewConnection(IPEndPoint endpoint) {
            ConnectedClients[endpoint] = new UserData();
            ConnectedClients[endpoint].ID = Random.Next();
        }

        private void ServerMessageReceived(NetMessage message) {
            ServersideLobbyState.HandleMessage(message, this);
        }
    }

    class ServersideLobbyState : LobbyState {
        public override Dictionary<Type, NetMessageHandler> MessageHandlers => new Dictionary<Type, NetMessageHandler>() {
            { typeof(RequestConnection), (m, l, s) => {
                var rc = (RequestConnection)m;
                if (!l.AlreadyConnected(rc.Sender)) {
                    l.RegisterNewConnection(rc.Sender);
                    NetHandler.SendToClient(rc.Sender, new AllowConnection(l.GetConnectedClientID(rc.Sender)));

                    // send list of already connected clients
                    List<int> ids = new List<int>();
                    List<string> names = new List<string>();
                    foreach (UserData data in l.ConnectedClients.Values) {
                        ids.Add(data.ID);
                        names.Add(data.Name ?? ReservedWords.Unset);
                    }
                    NetHandler.SendToClient(rc.Sender, new PlayerList(ids.ToArray()));
                    NetHandler.SendToClient(rc.Sender, new NamesList(ids.ToArray(), names.ToArray()));

                    // send world


                    // inform all of new client
                    NetHandler.SendToAllClients(new PlayerList(l.GetConnectedClientID(rc.Sender)));
                }
            } },
            { typeof(TextMessage), (m, l, s) => {
                var tm = (TextMessage)m;
                Logger.Log($"s rcv {tm.Text} [{tm.OriginatingID}]");
                NetHandler.SendToAllClients(new TextMessage(tm.Text, l.GetConnectedClientID(tm.Sender)));
            } },
            { typeof(SetName), (m, l, s) => {
                var sn = (SetName)m;
                int senderID = l.GetConnectedClientID(m.Sender);
                Logger.Log($"{senderID} would like to be called {sn.Name}");

                if (ReservedWords.IsWordReserved(sn.Name)) {
                    Logger.Log($"\t{sn.Name} is reserved, disallowing");
                    NetHandler.SendToClient(m.Sender, new DisallowName());
                }
                else {
                    Logger.Log($"\t{sn.Name} is allowed");
                    l.ConnectedClients[m.Sender].Name = sn.Name;

                    // inform all connected clients of name
                    NetHandler.SendToAllClients(new NamesList(new[]{ senderID }, new[]{ sn.Name }));
                }
            } },
        };

        public override bool ExpectingMessageOfType(Type t) {
            if (!NetMessage.ExpectedOnServer(t)) {
                return false;
            }
            return true;
        }
    }
}
