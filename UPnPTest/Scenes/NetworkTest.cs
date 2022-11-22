using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedSurvivalGame.Networking;
using TileBasedSurvivalGame.Networking.Messages;

using IPEndPoint = System.Net.IPEndPoint;

namespace TileBasedSurvivalGame.Scenes {
    internal class NetworkTest : Scene {
        Random rng = new Random();

        public override string Name => "Network Test [local server]";
        public override Scene Next { get => this; protected set { } }

        public Dictionary<IPEndPoint, UserData> ConnectedClients { get; }
        = new Dictionary<IPEndPoint, UserData>();
        public Dictionary<int, UserData> RemotePlayers { get; }
        = new Dictionary<int, UserData>();

        int ClientID { get; set; }
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
            ConnectedClients[endpoint].ID = rng.Next();
        }

        public override void Begin(Engine instance) {
            Console.WriteLine("is this instance a host?");
            bool host = Console.ReadKey().Key == ConsoleKey.Y;

            Logger.ShowLogs = true;
            NetHandler.Setup(System.Net.IPAddress.Parse("127.0.0.1"), 12000, true, host);

            NetHandler.ServerMessage += ServerMessageReceived;
            NetHandler.ClientMessage += ClientMessageReceived;

            NetHandler.SendToServer(new RequestConnection());
        }

        private void ClientMessageReceived(NetMessage message) {
            if (message is AllowConnection ac) {
                ClientID = ac.ClientID;
                Logger.Log($"server accepted connection! my ID is {ClientID}");
                NetHandler.SendToServer(new TextMessage("hi! I am the client."));
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
                    if(id == ClientID) {
                        continue;
                    }

                    Logger.Log($"new connection: \t{id}");
                    RemotePlayers[id] = new UserData();
                    RemotePlayers[id].ID = id;
                }
            }
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

        public override void Render(Engine instance) {
        }

        public override void Tick(Engine instance) {
            NetHandler.SendToServer(new TextMessage(Console.ReadLine()));
        }

        public override void Update(Engine instance, float elapsed) {
        }
    }
}
