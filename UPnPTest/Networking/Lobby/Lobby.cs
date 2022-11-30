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
                ClientData = new UserData();
                ClientsideLobbyState = new ClientsideLobbyState();
            }
            if (server) {
                NetHandler.ServerMessage += ServerMessageReceived;
                ServerWorld = new World.World(100);
                ServersideLobbyState = new ServersideLobbyState();
            }

            Logger.ShowLogs = true;
            NetHandler.Setup(System.Net.IPAddress.Parse("127.0.0.1"), 12000, true, server);
            NetHandler.SendToServer(new RequestConnection());
        }
    }

    abstract class LobbyState {
        public delegate void NetMessageHandler(NetMessage message, Lobby lobby, LobbyState state);
        public ConnectionState State { get; set; }
        public bool ExpectingMessageOfType<T>() { return ExpectingMessageOfType(typeof(T)); }
        public abstract bool ExpectingMessageOfType(Type t);

        public abstract Dictionary<Type, NetMessageHandler> MessageHandlers { get; }

        public void HandleMessage(NetMessage message, Lobby lobby) {
            var msg = NetMessage.MessageToSubtype(message);
            Type subtype = msg.GetType();
            if (ExpectingMessageOfType(subtype) && MessageHandlers.ContainsKey(subtype)) {
                MessageHandlers[subtype](msg, lobby, this);
            }
        }
    }
}
