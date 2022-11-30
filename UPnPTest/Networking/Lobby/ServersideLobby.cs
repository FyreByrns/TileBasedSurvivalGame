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
                    foreach (UserData data in l.ConnectedClients.Values) {
                        ids.Add(data.ID);
                    }
                    NetHandler.SendToClient(rc.Sender, new PlayerList(ids.ToArray()));

                    // inform all of new client
                    NetHandler.SendToAllClients(new PlayerList(l.GetConnectedClientID(rc.Sender)));
                }
            } },
            { typeof(TextMessage), (m, l, s) => { 
                var tm = (TextMessage)m;
                Logger.Log($"s rcv {tm.Text} [{tm.OriginatingID}]");
                NetHandler.SendToAllClients(new TextMessage(tm.Text, l.GetConnectedClientID(tm.Sender)));
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
