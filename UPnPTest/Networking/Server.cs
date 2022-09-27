using System;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

using FSerialization;

//// = documentation
// = per-step working comments

using PlayerID = System.Int32;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;
using static TileBasedSurvivalGame.Networking.Server.ServersideClientState;
using ByteList = System.Collections.Generic.List<byte>;
using System.Linq;
using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Networking {
    class Server : ITickable {
        public enum ServersideClientState {
            None,

            ResolvingName,
            InLobby,

        }

        Random _rng;

        // lobby data
        List<Player> _players = new List<Player>();
        Dictionary<PlayerID, IPEndPoint> _endpoints;
        Dictionary<PlayerID, ServersideClientState> _states;
        Dictionary<IPEndPoint, PlayerID> _IDs;

        // world data
        TiledWorld World { get; }
        = new TiledWorld();

        Player GetPlayerByID(PlayerID id) {
            foreach (Player player in _players) {
                if (player.ID == id) {
                    return player;
                }
            }

            return null;
        }

        //// send a message to all connected clients
        public void SendToAll(NetMessage message) {
            foreach (IPEndPoint endpoint in _endpoints.Values) {
                NetHandler.SendToClient(endpoint, message);
            }
        }
        public void SendToAll(byte[] data) {
            SendToAll(new NetMessage() { RawData = data });
        }

        public int GetNumberOfPlayers() {
            return _players.Count;
        }
        public IEnumerable<PlayerID> GetPlayerIDs() {
            return _endpoints.Keys;
        }

        void RegisterNewClient(PlayerID playerID, NetMessage originator) {
            _endpoints[playerID] = originator.Sender;
            _IDs[originator.Sender] = playerID;
            _states[playerID] = ResolvingName;
            _players.Add(new Player() { ID = playerID });
        }

        void UpdateConnectionState(NetMessage originatingMessage) {
            if (originatingMessage == null) { return; }

            // asynchronously handle state change
            Task.Run(() => {
                PlayerID playerID = _IDs[originatingMessage.Sender];
                ServersideClientState playerState = _states[playerID];
                Player player = GetPlayerByID(playerID);

                switch (playerState) {
                    case ResolvingName: {
                            int readIndex = 0;
                            string requestedName = originatingMessage.RawData.Get<string>(ref readIndex);
                            Logger.Log($"requested name: {requestedName}");
                            if (ReservedWords.IsWordReserved(requestedName)) {
                                // word is reserved, don't allow the name
                                NetHandler.SendToClient(originatingMessage.Sender, NetMessage.ConstructToSend(DenyDesiredName));
                                break;
                            }
                            // otherwise allow the name
                            player.Name = requestedName;
                            NetHandler.SendToClient(originatingMessage.Sender, NetMessage.ConstructToSend(AllowDesiredName));
                            // update client state
                            _states[playerID] = InLobby;

                            // inform others of join
                            ByteList data = new ByteList();
                            data.Append(playerID);
                            data.Append(requestedName);
                            SendToAll(NetMessage.ConstructToSend(PlayerJoin, data.ToArray()));
                            break;
                        }
                    case InLobby: {
                            // nested switch statements my beloved
                            switch (originatingMessage.MessageIntent) {
                                case RequestLobbyInfo: {
                                        ByteList data = new ByteList();
                                        data.Append(GetNumberOfPlayers());

                                        foreach (Player p in _players) {
                                            data.Append(p.ID);
                                        }

                                        NetHandler.SendToClient(originatingMessage.Sender, NetMessage.ConstructToSend(
                                            SendLobbyInfo,
                                            data.ToArray()
                                            ));
                                    }
                                    break;
                                case RequestName: {
                                        int readIndex = 0;
                                        PlayerID desiredID = originatingMessage.RawData.Get<PlayerID>(ref readIndex);
                                        Player desiredPlayer = GetPlayerByID(desiredID);

                                        if (desiredPlayer != null && desiredPlayer.HasName) {
                                            ByteList data = new ByteList();
                                            data.Append(desiredID);
                                            data.Append(desiredPlayer.Name);
                                            NetHandler.SendToClient(originatingMessage.Sender, NetMessage.ConstructToSend(
                                                SendName,
                                                data.ToArray()
                                                ));
                                        }

                                        break;
                                    }
                                case ClientTileChange: {
                                        int readIndex = 0;

                                        int globalX = originatingMessage.RawData.Get<int>(ref readIndex);
                                        int globalY = originatingMessage.RawData.Get<int>(ref readIndex);
                                        int globalZ = originatingMessage.RawData.Get<int>(ref readIndex);
                                        Location global = new Location(globalX, globalY);
                                        string tile = originatingMessage.RawData.Get<string>(ref readIndex);
                                        World.SetTile(Location.ToChunk(global), Location.ToTile(global), TileTypeHandler.CreateTile(tile));

                                        ByteList data = new ByteList();
                                        data.Append(playerID);
                                        data.Append(globalX);
                                        data.Append(globalY);
                                        data.Append(globalZ);
                                        data.Append(tile);
                                        SendToAll(NetMessage.ConstructToSend(ServerTileChange, data.ToArray()));

                                        break;
                                    }
                            }

                            break;
                        }
                }
            });
        }

        public void Tick(Engine context) {
            World.Tick(context);
        }

        public Server() {
            _rng = new Random((int)DateTime.Now.Ticks);
            NetHandler.ServerMessage += Server_MessageReceived;

            _endpoints = new Dictionary<PlayerID, IPEndPoint>();
            _IDs = new Dictionary<IPEndPoint, PlayerID>();
            _states = new Dictionary<PlayerID, ServersideClientState>();
            _players = new List<Player>();
        }

        private void Server_MessageReceived(NetMessage message) {
            // if the message is from a blocked endpoint, discard
            if (message == NetMessage.BlockedMessage) {
                return;
            }
            // if the intent is to ping, pong
            if (message.MessageIntent == Ping) {
                NetHandler.SendToClient(message.Sender,
                    NetMessage.ConstructToSend(Ping));
                return;
            }

            // if the message is from a known client
            if (_IDs.ContainsKey(message.Sender)) {
                // handle the message based on game state, intent, etc
                Logger.Log($"[s] rcv msg from {message.Sender}[{_IDs[message.Sender]}]");
                Logger.Log($"  intent: {message.MessageIntent}");
                UpdateConnectionState(message);
            }
            // otherwise ..
            else {
                Logger.Log($"[s] new connection from {message.Sender}!");

                // .. if the message intent is to begin a connection, 
                // .. and a connection is allowed, begin a connection
                if (message.MessageIntent == RequestConnection) {
                    // generate ID for player
                    PlayerID newID = _rng.Next(PlayerID.MaxValue);
                    // ensure uniqueness with dumb loop, there are better ways
                    // .. but I'm using system random anyway so better ways
                    // .. don't seem entirely relevant
                    while (_endpoints.ContainsKey(newID)) {
                        newID = _rng.Next(PlayerID.MaxValue);
                    }
                    // add player to lobby
                    RegisterNewClient(newID, message);

                    // allow connection
                    ByteList data = new ByteList();
                    data.Append(newID);
                    NetHandler.SendToClient(message.Sender, NetMessage.ConstructToSend(AllowConnection, data.ToArray()));

                    // handle message now that client is known
                    NetHandler.OnServerMessage(message);
                    return;
                }
            }
        }
    }
}
