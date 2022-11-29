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
        public Dictionary<IPEndPoint, UserData> ConnectedClients { get; }
        = new Dictionary<IPEndPoint, UserData>();

        public World.World ServerWorld { get; set; }

        int GetConnectedClientID(IPEndPoint endpoint) {
            if (ConnectedClients.ContainsKey(endpoint)) {
                return ConnectedClients[endpoint].ID;
            }
            return -1;
        }
        bool AlreadyConnected(IPEndPoint endpoint) {
            return ConnectedClients.ContainsKey(endpoint);
        }
        void RegisterNewConnection(IPEndPoint endpoint) {
            ConnectedClients[endpoint] = new UserData();
            ConnectedClients[endpoint].ID = Random.Next();
        }

        private void ServerMessageReceived(NetMessage message) {
            if (message is RequestConnection) {
                if (!AlreadyConnected(message.Sender)) {
                    RegisterNewConnection(message.Sender);
                    NetHandler.SendToClient(message.Sender, new AllowConnection(GetConnectedClientID(message.Sender)));

                    // send list of already connected clients
                    List<int> ids = new List<int>();
                    foreach (UserData data in ConnectedClients.Values) {
                        ids.Add(data.ID);
                    }
                    NetHandler.SendToClient(message.Sender, new PlayerList(ids.ToArray()));

                    // inform all of new client
                    NetHandler.SendToAllClients(new PlayerList(GetConnectedClientID(message.Sender)));
                }

            }

            if (message is TextMessage textMessage) {
                Logger.Log($"s rcv {textMessage.Text} [{textMessage.OriginatingID}]");
                NetHandler.SendToAllClients(new TextMessage(textMessage.Text, GetConnectedClientID(message.Sender)));
            }
        }
    }
}
