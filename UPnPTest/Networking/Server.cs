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
using TileBasedSurvivalGame.StateMachines.ServersideConnectionState;
using TileBasedSurvivalGame.StateMachines.ServersideConnectionState.States;

namespace TileBasedSurvivalGame.Networking {
    class Server {
        Random _rng;

        // lobby data
        LobbyData _lobbyData;
        Dictionary<PlayerID, IPEndPoint> _endpointsByID;
        Dictionary<IPEndPoint, PlayerID> _IDsByEndpoint;
        Dictionary<PlayerID, ServersideConnectionStateMachine> _statesByID;

        public void Respond(NetMessage originalMessage, NetMessage response) {
            NetHandler.SendTo(originalMessage.Sender, response);
        }

        public Server() {
            _rng = new Random((int)DateTime.Now.Ticks);
            NetHandler.ServerMessage += Server_MessageReceived;

            _endpointsByID = new Dictionary<PlayerID, IPEndPoint>();
            _IDsByEndpoint = new Dictionary<IPEndPoint, PlayerID>();
            _lobbyData = new LobbyData();
            _statesByID = new Dictionary<PlayerID, ServersideConnectionStateMachine>();
        }

        void RegisterNewClient(PlayerID playerID, IPEndPoint endpoint) {
            _endpointsByID[playerID] = endpoint;
            _IDsByEndpoint[endpoint] = playerID;
            _lobbyData.AddPlayer(playerID, new Player());

            _statesByID[playerID] = new ServersideConnectionStateMachine(this);
            _statesByID[playerID].CurrentState = new InitiatingConnection();
            _statesByID[playerID].Enter();
        }

        private void Server_MessageReceived(NetMessage message) {
            // if the message is from a blocked endpoint, discard
            if (message == NetMessage.BlockedMessage) {
                return;
            }

            // if the message is from a known client
            if (_IDsByEndpoint.ContainsKey(message.Sender)) {
                // handle the message based on game state, intent, etc
                _statesByID[_IDsByEndpoint[message.Sender]].Update(message, _statesByID[_IDsByEndpoint[message.Sender]]);
                
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
