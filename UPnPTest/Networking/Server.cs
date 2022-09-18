using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using FSerialization;

//// = documentation
// = per-step working comments

using PlayerID = System.Int32;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;
using TileBasedSurvivalGame.StateMachines;
using TileBasedSurvivalGame.StateMachines.Serverside;
using TileBasedSurvivalGame.StateMachines.Serverside.ConnectionStates;

namespace TileBasedSurvivalGame.Networking {
    class Server {
        Random _rng;

        // lobby data
        LobbyData _lobbyData;
        Dictionary<PlayerID, IPEndPoint> _endpointsByID;
        Dictionary<IPEndPoint, PlayerID> _IDsByEndpoint;
        Dictionary<PlayerID, ConnectionStateMachine> _statesByID;

        public int GetNumberOfPlayers() {
            return _lobbyData.Players.Count;
        }
        public IEnumerable<PlayerID> GetPlayerIDs() {
            return _endpointsByID.Keys;
        }

        public Server() {
            _rng = new Random((int)DateTime.Now.Ticks);
            NetHandler.ServerMessage += Server_MessageReceived;

            _endpointsByID = new Dictionary<PlayerID, IPEndPoint>();
            _IDsByEndpoint = new Dictionary<IPEndPoint, PlayerID>();
            _statesByID = new Dictionary<PlayerID, ConnectionStateMachine>();
            _lobbyData = new LobbyData();
        }

        void RegisterNewClient(PlayerID playerID, NetMessage originator) {
            _endpointsByID[playerID] = originator.Sender;
            _IDsByEndpoint[originator.Sender] = playerID;
            _lobbyData.AddPlayer(playerID, new Player());

            _statesByID[playerID] = new ConnectionStateMachine();
            _statesByID[playerID].CurrentState = new ResolvingName(_statesByID[playerID]);
        }

        void UpdateConnectionState(NetMessage originatingMessage) {
            if (originatingMessage == null) { return; }

            // asynchronously handle state change
            Task.Run(() => {
                PlayerID playerID = _IDsByEndpoint[originatingMessage.Sender];
                ConnectionStateMachine connectionStateMachine = _statesByID[playerID];
                Console.WriteLine($"c[{playerID}]: {connectionStateMachine.CurrentState.GetType().Name}");
                StateMachineHandler.Update(connectionStateMachine, originatingMessage);
                Console.WriteLine($"n[{playerID}]: {connectionStateMachine.CurrentState.GetType().Name}");
            });
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
            if (_IDsByEndpoint.ContainsKey(message.Sender)) {
                // handle the message based on game state, intent, etc
                Console.WriteLine($"[s] rcv msg from {message.Sender}[{_IDsByEndpoint[message.Sender]}]");
                Console.WriteLine($"  intent: {message.MessageIntent}");
                UpdateConnectionState(message);

                // message is done with, read state may be cleared
                message.RawData.Done();
            }
            // otherwise ..
            else {
                Console.WriteLine($"[s] new connection from {message.Sender}!");

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
                    RegisterNewClient(newID, message);

                    // allow connection
                    NetHandler.SendToClient(message.Sender, NetMessage.ConstructToSend(AllowConnection));

                    // handle message using state machine
                    NetHandler.OnServerMessage(message);
                    return;
                }
            }
        }
    }
}
