using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;

using FSerialization;

//// = documentation
// = per-step working comments

using PlayerID = System.Int32;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;

namespace TileBasedSurvivalGame.Networking {
    //// to keep track of all reserved / disallowed words
    static class ReservedWords {
        public static string Unset { get; } = "[unset]";

        //// whether a word is allowed
        public static bool WordIsAllowed(string word) {
            return !_reservedWords.Contains(word);
        }

        #region functionality
        static List<string> _reservedWords = new List<string>();

        public static bool IsWordReserved(string word) {
            return _reservedWords.Contains(word);
        }

        static ReservedWords() {
            // automagically get all string properties of this static class
            // .. and add them to the reserved list
            foreach (PropertyInfo propertyInfo in typeof(ReservedWords).GetProperties()) {
                if (propertyInfo.PropertyType == typeof(string)) {
                    _reservedWords.Add((string)propertyInfo.GetValue(null));
                }
            }
        }
        #endregion functionality
    }

    class Player {
        public string Name { get; private set; }
            = ReservedWords.Unset;
        public bool HasName { get; private set; }
    }

    class LobbyData {
        public Dictionary<PlayerID, Player> Players { get; }
            = new Dictionary<PlayerID, Player>();

        public void AddPlayer(PlayerID playerID, Player player) {
            Players[playerID] = player;
        }
        public Player GetPlayer(PlayerID playerID) {
            if (Players.ContainsKey(playerID)) {
                return Players[playerID];
            }
            return null;
        }
    }

    class Server {
        public event NetMessageEventHandler MessageReceived;
        public int Port { get; }
        UdpClient _udpClient;
        Random _rng;

        // lobby data
        LobbyData _lobbyData;
        Dictionary<PlayerID, IPEndPoint> _endpointsByID;
        Dictionary<IPEndPoint, PlayerID> _IDsByEndpoint;

        void Listen() {
            while (true) {
                // wait for client connection
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, Port);
                byte[] rcvData = _udpClient.Receive(ref sender);

                OnReceive(NetMessage.ConstructFromSent(sender, rcvData));
            }
        }

        void Respond(NetMessage originalMessage, NetMessage response) {
            Task.Run(() => {
                _udpClient.SendAsync(response.RawData, response.RawData.Length, originalMessage.Sender);
            });
        }

        void OnReceive(NetMessage message) {
            MessageReceived?.Invoke(message);
        }

        public void Start() {
            // init random
            _rng = new Random((int)DateTime.Now.Ticks);

            // create the client
            _udpClient = new UdpClient(Port);
            _lobbyData = new LobbyData();

            _endpointsByID = new Dictionary<PlayerID, IPEndPoint>();
            _IDsByEndpoint = new Dictionary<IPEndPoint, PlayerID>();

            // start listening thread
            Task.Run(Listen);
        }

        void RegisterNewClient(PlayerID playerID, IPEndPoint endpoint) {
            _endpointsByID[playerID] = endpoint;
            _IDsByEndpoint[endpoint] = playerID;
            _lobbyData.AddPlayer(playerID, new Player());
        }

        public Server(int port) {
            Port = port;

            MessageReceived += Server_MessageReceived;
        }

        private void Server_MessageReceived(NetMessage message) {
            // if the message is from a blocked endpoint, discard
            if (message == NetMessage.BlockedMessage) {
                return;
            }

            // if the message is from a known client
            if (_IDsByEndpoint.ContainsKey(message.Sender)) {
                // handle the message based on game state, intent, etc
                switch (message.MessageIntent) {
                    case SendString:
                        break;
                    case SendNumber:
                        break;

                    case RequestDesiredName:
                        // if the client requesting a name doesn't have a name ..
                        if (!_lobbyData.GetPlayer(_IDsByEndpoint[message.Sender]).HasName) {
                            // .. and the requested name is allowed
                            string requestedName = message.RawData.Get<string>();
                            if (ReservedWords.WordIsAllowed(requestedName)) {
                                Respond(message,
                                    NetMessage.ConstructToSend(AllowConnection));
                            }
                        }
                        break;

                    case RequestLobbyInfo:
                        break;
                    case RequestPlayerInfo:
                        break;
                    default:
                        break;
                }

                // message is done with, read state may be cleared
                message.RawData.Done();
            }
            // otherwise ..
            else {
                // .. if the message intent is to ping, send back a pong
                if (message.MessageIntent == Ping) {
                    Respond(message,
                        NetMessage.ConstructToSend(Ping));
                    return;
                }

                // .. if the message intent is to begin a connection, 
                // .. and a connection is allowed, begin a connection
                if (message.MessageIntent == RequestConnection) {
                    // generate ID for player
                    PlayerID newID = _rng.Next(PlayerID.MaxValue);
                    // ensure uniqueness with dumb loop, there are better ways
                    // .. but I'm using system random anyway so better ways
                    // .. don't seem entirely relevant
                    while (_endpointsByID.ContainsKey(newID)) {
                        newID = _rng.Next(PlayerID.MaxValue);
                    }
                    // add player to lobby
                    RegisterNewClient(newID, message.Sender);

                    // respond with AllowConnection
                    Respond(message,
                        NetMessage.ConstructToSend(AllowConnection));
                    return;
                }
            }
        }
    }
}
