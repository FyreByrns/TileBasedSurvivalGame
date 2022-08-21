using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {
    //// static class to handle all the innards of networking
    static class NetHandler {
        public static event NetMessageEventHandler ServerMessage;
        public static event NetMessageEventHandler ClientMessage;

        public static bool HasServer { get; private set; }
        public static bool HasClient { get; private set; }
        public static IPAddress ServerIP { get; private set; }
        public static int ServerPort { get; private set; }

        private static UdpClient _serverListener;
        private static UdpClient _clientListener;
        private static UdpClient _sender;

        public static void OnServerMessage(NetMessage message) {
            ServerMessage?.Invoke(message);
        }
        public static void OnClientMessage(NetMessage message) {
            ClientMessage?.Invoke(message);
        }

        public static void SendTo(IPEndPoint target, NetMessage message) {
            // make sure sender UdpClient exists
            if (_sender == null) {
                _sender = new UdpClient();
                _sender.Client.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            }

            Task.Run(() => {
                int sent = _sender.Send(message.RawData, message.RawData.Length, target);

                if (sent != message.RawData.Length) {
                    // something went wrong
                    Console.WriteLine($"tried to send {message.RawData.Length}, sent {sent} instead");
                }
            });
        }

        public static void SendToServer(NetMessage message) {
            SendTo(new IPEndPoint(ServerIP, ServerPort), message);
        }

        static void StartListening(bool client, bool server) {
            HasClient = client;
            HasServer = server;

            // if there's a need to handle the server
            if (HasServer) {
                // make sure the listener exists
                if (_serverListener == null) {
                    _serverListener = new UdpClient(ServerPort);
                }

                // start server listening
                Task.Run(() => {
                    while (true) {
                        // receive data
                        IPEndPoint sender = null;
                        byte[] receivedData = _serverListener.Receive(ref sender);

                        OnServerMessage(NetMessage.ConstructFromSent(sender, receivedData));
                    }
                });
            }

            // if there's a need to handle a client
            if (HasClient) {
                // make sure the client listener exists
                if (_clientListener == null) {
                    _clientListener = new UdpClient();
                    _clientListener.Connect(new IPEndPoint(ServerIP, ServerPort));
                }

                // start client listening
                Task.Run(() => {
                    while (true) {
                        // receive data
                        IPEndPoint sender = null;
                        byte[] receivedData = _clientListener.Receive(ref sender);

                        OnClientMessage(NetMessage.ConstructFromSent(sender, receivedData));
                    }
                });
            }
        }

        public static void Setup(IPAddress serverIP, int serverPort, bool client, bool server) {
            ServerIP = serverIP;
            ServerPort = serverPort;
            StartListening(client, server);
        }
    }
}
