//#define EXCESSIVE_DEBUG_PRINTS

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TileBasedSurvivalGame.Networking {

    //// static class to handle all the innards of networking
    static class NetHandler {
        public delegate void NetMessageEventHandler(NetMessage message);

        public static event NetMessageEventHandler ServerMessage;
        public static event NetMessageEventHandler ClientMessage;

        public static IPAddress ServerIP { get; private set; }
        public static int ServerPort { get; private set; }
        public static IPEndPoint ServerEP => new IPEndPoint(ServerIP, ServerPort);

        public static HashSet<IPEndPoint> ConnectedClients { get; private set; }
        = new HashSet<IPEndPoint>();

        private static UdpClient server;
        private static UdpClient client;

        public static void OnServerMessage(NetMessage message) {
            ServerMessage?.Invoke(message);
        }
        public static void OnClientMessage(NetMessage message) {
            ClientMessage?.Invoke(message);
        }

        public static void SendToClient(IPEndPoint target, NetMessage message) {
            Task.Run(() => {
                Logger.Log($"to {target}: {message.Intent}");

                server.Send(message.RawData, message.RawData.Length, target);
            });
        }
        public static void SendToAllClients(NetMessage message) {
            foreach (IPEndPoint target in ConnectedClients) {
                SendToClient(target, message);
            }
        }

        public static void SendToServer(NetMessage message) {
            Task.Run(() => {
                Logger.Log($"to server: {message.Intent}");

                client.Send(message.RawData, message.RawData.Length, ServerEP);
            });
        }

        private static void ServerDataReceived(IAsyncResult result) {
            UdpClient self = (UdpClient)result.AsyncState;

            IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = self.EndReceive(result, ref from);
            ConnectedClients.Add(from);

            NetMessage message = new NetMessage(from, data);
            message = NetMessage.MessageToSubtype(message);

            OnServerMessage(message);

            self.BeginReceive(ServerDataReceived, result.AsyncState);
        }
        private static void ClientDataReceived(IAsyncResult result) {
            UdpClient self = (UdpClient)result.AsyncState;

            IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = self.EndReceive(result, ref from);

            NetMessage message = new NetMessage(from, data);
            message = NetMessage.MessageToSubtype(message);

            OnClientMessage(message);

            self.BeginReceive(ClientDataReceived, result.AsyncState);
        }
        static void StartListening(bool client, bool server) {
            if (server) {
                NetHandler.server = new UdpClient(ServerPort);
                NetHandler.server.BeginReceive(ServerDataReceived, NetHandler.server);
            }
            if (client) {
                NetHandler.client = new UdpClient(0);
                NetHandler.client.BeginReceive(ClientDataReceived, NetHandler.client);
            }
        }

        public static void Setup(IPAddress serverIP, int serverPort, bool client, bool server) {
            ServerIP = serverIP;
            ServerPort = serverPort;
            StartListening(client, server);

            ServerMessage += NetHandler_Message;
            ClientMessage += NetHandler_Message;
        }

        private static void NetHandler_Message(NetMessage message) {
            if (message == null) {
                Logger.Log("null message");
                return;
            }

            Logger.Log($"fr {(message.Sender.Equals(ServerEP) ? "server" : message.Sender.ToString())} {message.Intent}");
        }
    }
}
