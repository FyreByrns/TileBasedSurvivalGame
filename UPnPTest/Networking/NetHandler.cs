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
        public static IPEndPoint ServerEP => new IPEndPoint(ServerIP, ServerPort);

        public static IPAddress ClientIP { get; private set; }
        public static int ClientPort { get; private set; }
        public static IPEndPoint ClientEP => new IPEndPoint(ClientIP, ClientPort);

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
            foreach(byte b in message.RawData.Skip(sizeof(long))) {
                Console.Write($"{b,4}");
            }
            Console.WriteLine();
            foreach(byte b in message.RawData.Skip(sizeof(long))) {
                Console.Write($"{(char)b,4}");
            }
            Console.WriteLine();
            //for(int i = sizeof(long); i < message.RawData.Length; i += 4) {
                //Console.Write($"{BitConverter.ToInt32(message.RawData, i),16}");
            //}
            //Console.WriteLine();

            // direct-pipe straight to client, if there's an integrated server
            if (HasClient && target.Equals(ClientEP)) {
                NetMessage.SetSender(ref message, ServerEP);
                Console.WriteLine(message.MessageIntent);
                OnClientMessage(NetMessage.ConstructFromSent(ServerEP, message.RawData));
                return;
            }
            // direct-pipe straight to integrated server
            if(HasServer && target.Equals(ServerEP)) {
                NetMessage.SetSender(ref message, ClientEP);
                OnServerMessage(NetMessage.ConstructFromSent(ClientEP, message.RawData));
                return;
            }

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
            SendTo(ServerEP, message);
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

                    ClientIP = ((IPEndPoint)_clientListener.Client.LocalEndPoint).Address;
                    ClientPort = ((IPEndPoint)_clientListener.Client.LocalEndPoint).Port;
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

            ServerMessage += NetHandler_Message;
            ClientMessage += NetHandler_Message;
        }

        private static void NetHandler_Message(NetMessage message) {
            if (message == null) {
                Console.WriteLine("null message");
                return;
            }

            Console.WriteLine($"[{(message.Sender == ServerEP ? "s" : "c")}][{message.Sender}][{message.Latency.TotalMilliseconds}ms][{message.MessageIntent}]");
        }
    }
}
