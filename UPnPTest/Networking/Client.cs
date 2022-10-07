﻿using PixelEngine;
using System;
using System.Linq;
using FSerialization;

//// = documentation
// = per-step working comments

using static TileBasedSurvivalGame.Networking.Client.ClientsideClientState;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;
using ByteList = System.Collections.Generic.List<byte>;
using PlayerList = System.Collections.Generic.List<TileBasedSurvivalGame.Networking.Player>;
using TileBasedSurvivalGame.World;
using TileBasedSurvivalGame.Rendering;

namespace TileBasedSurvivalGame.Networking {
    class Client : ITickable {
        public enum ClientsideClientState {
            None,

            ResolvingName,

            EnteringLobby,
            InLobby,

            EnteringWorld,
            InWorld,
        }
        public ClientsideClientState CurrentState { get; private set; }
        public int MyID { get; private set; }
        // ugly invisible state
        public NetMessage MostRecentMessage { get; private set; }
        PlayerList Players = new PlayerList();

        public TiledWorld World { get; set; }
        = new TiledWorld();
        public Location CameraLocation { get; set; }
        = Location.Zero;
        public Camera Camera { get; set; }

        public Player GetPlayerByID(int id) {
            foreach (Player player in Players) {
                if (player.ID == id) {
                    return player;
                }
            }
            return null;
        }

        public void UpdateState() {
            if (MostRecentMessage?.MessageIntent == PlayerJoin) {
                int readIndex = 0;

                int id = MostRecentMessage.RawData.Get<int>(ref readIndex);
                Players.Add(new Player(id, id != MyID));
                Player player = GetPlayerByID(id);
                if (player == null) {
                    return;
                }
                player.Name = MostRecentMessage.RawData.Get<string>(ref readIndex);

                Logger.Log("players:");
                foreach (Player p in Players) {
                    Logger.Log($"\t{p.ID}:{p.Remote}:{p.Name}");
                }

                return;
            }

            // always do these things
            #region stateless actions

            if (MostRecentMessage?.MessageIntent == SendName) {
                int readIndex = 0;
                int id = MostRecentMessage.RawData.Get<int>(ref readIndex);
                Player player = GetPlayerByID(id);
                player.Name = MostRecentMessage.RawData.Get<string>(ref readIndex);
            }

            if (MostRecentMessage?.MessageIntent == PlayerJoin) {
                int readIndex = 0;
                int id = MostRecentMessage.RawData.Get<int>(ref readIndex);
                Player player = GetPlayerByID(id);
                if (player == null) {
                    player = new Player(id, id != MyID);
                    Players.Add(player);
                }
            }

            if (MostRecentMessage?.MessageIntent == PlayerSpawn) {
                int readIndex = 0;
                int id = MostRecentMessage.Get<int>(ref readIndex);
                int globalX = MostRecentMessage.RawData.Get<int>(ref readIndex);
                int globalY = MostRecentMessage.RawData.Get<int>(ref readIndex);

                // spawn in
                Entity player = new Entity(new Location(globalX, globalY));
                player.Controller = new PlayerController(player);
                World.Entities.Add(player);
                GetPlayerByID(id).Entity = player;
            }

            #endregion stateless actions

            // stateful actions
            switch (CurrentState) {
                case ClientsideClientState.None: {
                        if (MostRecentMessage == null) {
                            // no message from the server has been sent, so request a connection
                            NetHandler.SendToServer(NetMessage.ConstructToSend(RequestConnection));
                        }
                        else {
                            // server might have responded to the connection request
                            if (MostRecentMessage?.MessageIntent == AllowConnection) {
                                // get my id from message
                                int readIndex = 0;
                                MyID = MostRecentMessage.RawData.Get<int>(ref readIndex);

                                // request a name
                                // .. get name TODO actually use proper menu
                                Console.Write("Enter your name: ");
                                string name = "unnamed";//Console.ReadLine();
                                ByteList data = new ByteList();
                                data.Append(name);
                                // .. send name request
                                NetHandler.SendToServer(NetMessage.ConstructToSend(
                                    RequestDesiredName,
                                    data.ToArray()
                                    ));
                                // .. update current state
                                CurrentState = ResolvingName;
                            }
                        }
                        break;
                    }

                case ResolvingName: {
                        if (MostRecentMessage?.MessageIntent == AllowDesiredName) {
                            // name allowed!
                            // request lobby information
                            NetHandler.SendToServer(NetMessage.ConstructToSend(RequestLobbyInfo));
                            // update current state
                            CurrentState = EnteringLobby;
                        }
                        break;
                    }

                case EnteringLobby: {
                        if (MostRecentMessage?.MessageIntent == SendLobbyInfo) {
                            int readIndex = 0;
                            int numberOfPlayers = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            Players = new PlayerList(numberOfPlayers);

                            // add players by IDs
                            for (int i = 0; i < numberOfPlayers; i++) {
                                int id = MostRecentMessage.RawData.Get<int>(ref readIndex);
                                Players.Add(new Player(id, id != MyID));
                            }
                            // request names
                            for (int i = 0; i < numberOfPlayers; i++) {
                                ByteList data = new ByteList();
                                data.Append(Players[i].ID);
                                NetHandler.SendToServer(NetMessage.ConstructToSend(
                                    RequestName,
                                    data.ToArray()
                                    ));
                            }

                            // initial lobby connection done, state -> InLobby
                            CurrentState = InLobby;
                        }
                        break;
                    }
                case InLobby: {
                        if (MostRecentMessage?.MessageIntent == ServerTileChange) {
                            int readIndex = 0;
                            int originatingID = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            int globalX = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            int globalY = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            int globalZ = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            Location global = new Location(globalX, globalY);
                            string tile = MostRecentMessage.RawData.Get<string>(ref readIndex);

                            World.SetTile(Location.ToChunk(global), Location.ToTile(global), TileTypeHandler.CreateTile(tile), false, true);
                        }

                        break;
                    }
            }
        }

        public void Tick(Engine context) {
            int camXChange = 0;
            int camYChange = 0;
            CameraLocation += new Location(camXChange, camYChange);
            if (CameraLocation.X < 0) CameraLocation = new Location(0, CameraLocation.Y);
            if (CameraLocation.Y < 0) CameraLocation = new Location(CameraLocation.X, 0);

            // handle state, state changes
            UpdateState();

            World.Tick(context);

            // center camera on self
            CameraLocation = GetPlayerByID(MyID)?.Entity?.WorldLocation
                - new Location(
                    context.ScreenWidth / TileRenderingHandler.TileSize / 2,
                    context.ScreenHeight / TileRenderingHandler.TileSize / 2
                    ) ?? Location.Zero;
        }

        public Client() {
            NetHandler.ClientMessage += NetHandler_ClientMessage;
            CurrentState = ClientsideClientState.None;
            Camera = new Camera();
            World.WorldChange += Camera.WorldChanged;
            World.EntityMoved += Camera.EntityMoved;
            World.WorldChange += World_WorldChange;

            // temporary debug world changing
            InputHandler.BindInput("mouse_left", Mouse.Left);
            InputHandler.Input += (string input, int held) => {
                if (input == "mouse_left") {
                    int ts = TileRenderingHandler.TileSize;
                    int mouseGlobalX = InputHandler.MouseX / ts;
                    int mouseGlobalY = InputHandler.MouseY / ts;
                    Location mouseChunk = Location.ToChunk(CameraLocation + new Location(mouseGlobalX, mouseGlobalY));
                    Location mouseTile = Location.ToTile(CameraLocation + new Location(mouseGlobalX, mouseGlobalY));

                    World.SetTile(mouseChunk, mouseTile, TileTypeHandler.CreateTile("test"));
                }
            };
        }

        private void World_WorldChange(Location chunkLoc, Location tileLoc, Tile tile, bool fromServer) {
            // send change to server
            Location global = Location.ToWorld(chunkLoc, tileLoc);
            ByteList data = new ByteList();
            data.Append(global.X);
            data.Append(global.Y);
            data.Append(tile.Type);

            if (!fromServer) {
                NetHandler.SendToServer(NetMessage.ConstructToSend(ClientTileChange, data.ToArray()));
            }
        }

        private void NetHandler_ClientMessage(NetMessage message) {
            // record most recent message, probably spawns race condition and will bite me later
            MostRecentMessage = message;
            // handle state, state changes
            UpdateState();
        }
    }
}
