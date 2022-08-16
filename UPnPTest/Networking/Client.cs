using PixelEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

//// = documentation
// = per-step working comments


namespace TileBasedSurvivalGame.Networking {
    class Client : Game {
        public event NetMessageEventHandler MessageReceived;
        public IPAddress ServerAddress { get; }
        public int ServerPort { get; }
        UdpClient _udpClient;

        void Listen() {
            while (true) {
                // wait for client connection
                IPEndPoint sender = new IPEndPoint(ServerAddress, ServerPort);
                byte[] rcvData = _udpClient.Receive(ref sender);

                OnReceive(NetMessage.ConstructFromSent(sender, rcvData));
            }
        }

        void OnReceive(NetMessage message) {
            MessageReceived?.Invoke(message);
        }

        public void SendToServer(NetMessage message) {
            _udpClient.Send(message.RawData, message.RawData.Length);
        }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);

            Draw(MouseX, MouseY, Pixel.Presets.Lime);

            if (GetMouse(Mouse.Left).Pressed) {
                SendToServer(NetMessage.ConstructToSend(NetMessage.Intent.Ping, new byte[0]));
            }
        }

        public Client(IPAddress serverAddress, int port) {
            ServerAddress = serverAddress;
            ServerPort = port;

            _udpClient = new UdpClient();
            _udpClient.Connect(new IPEndPoint(ServerAddress, ServerPort));

            Task.Run(() => { Listen(); });
        }
    }
}
