using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TileBasedSurvivalGame.Networking.Messages;

namespace TileBasedSurvivalGame.Networking {
    //// used to store information about a lobby
    partial class Lobby {
        public bool Client { get; set; }
        public bool Server { get; set; }

        public Random Random { get; private set; }
        public int RandomSeed { get; }

        public int RandomNumber() {
            return Random?.Next() ?? 0;
        }
        public void SetSeed(int seed) {
            Random = new Random(seed);
        }

        public Lobby(bool client, bool server) {
            Random seedSetter = new Random();
            RandomSeed = seedSetter.Next();
            SetSeed(RandomSeed);

            Console.WriteLine($"{seedSetter} {RandomSeed}");

            Client = client;
            Server = server;
            if (client) {
                NetHandler.ClientMessage += ClientMessageReceived;
            }
            if (server) {
                NetHandler.ServerMessage += ServerMessageReceived;
                ServerWorld = new World.World(100);
            }

            Logger.ShowLogs = true;
            NetHandler.Setup(System.Net.IPAddress.Parse("127.0.0.1"), 12000, true, server);
            NetHandler.SendToServer(new RequestConnection());
        }
    }

    abstract class LobbyState {
        public ConnectionState State { get; set; }
        public abstract bool ExpectingMessageOfType<T>() where T : NetMessage;
    }
}
