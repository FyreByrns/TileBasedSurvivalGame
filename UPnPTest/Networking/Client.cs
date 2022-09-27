using PixelEngine;
using System;
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
    class Client : Game {
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

        public TiledWorld World { get; }
        = new TiledWorld();
        public Location CameraLocation { get; set; }
        = Location.Zero;
        public Camera Camera { get; set; }

        Player GetPlayerByID(int id) {
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

                Console.WriteLine("players:");
                foreach (Player p in Players) {
                    Console.WriteLine($"\t{p.ID}:{p.Remote}:{p.Name}");
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
                        if(MostRecentMessage?.MessageIntent == ServerTileChange) {
                            int readIndex = 0;
                            int originatingID = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            int globalX = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            int globalY = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            int globalZ = MostRecentMessage.RawData.Get<int>(ref readIndex);
                            Location global = new Location(globalX, globalY, globalZ);
                            string tile = MostRecentMessage.RawData.Get<string>(ref readIndex);

                            World.SetTile(Location.ToChunk(global), Location.ToTile(global), TileTypeHandler.CreateTile(tile), originatingID == MyID, true);
                        }

                        break;
                    }
            }
        }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);

            int ts = TileRenderingHandler.TileSize;
            int mouseGlobalX = MouseX / ts;
            int mouseGlobalY = MouseY / ts;
            Location mouseChunk = Location.ToChunk(CameraLocation + new Location(mouseGlobalX, mouseGlobalY, 0));
            Location mouseTile = Location.ToTile(CameraLocation + new Location(mouseGlobalX, mouseGlobalY, 0));

            int camXChange = 0;
            int camYChange = 0;
            if (GetKey(Key.W).Down) { camYChange -= 1; }
            if (GetKey(Key.S).Down) { camYChange += 1; }
            if (GetKey(Key.A).Down) { camXChange -= 1; }
            if (GetKey(Key.D).Down) { camXChange += 1; }
            CameraLocation += new Location(camXChange, camYChange, 0);
            if (CameraLocation.X < 0) CameraLocation = new Location(0, CameraLocation.Y, CameraLocation.Z);
            if (CameraLocation.Y < 0) CameraLocation = new Location(CameraLocation.X, 0, CameraLocation.Z);
            if (CameraLocation.Z < 0) CameraLocation = new Location(CameraLocation.X, CameraLocation.Y, 0);


            if (GetMouse(Mouse.Left).Down) {
                World.SetTile(mouseChunk, mouseTile, TileTypeHandler.CreateTile("test"));
            }

            // handle state, state changes
            UpdateState();

            // render
            Camera.Render(this, CameraLocation);
        }

        public Client() {
            NetHandler.ClientMessage += NetHandler_ClientMessage;
            CurrentState = ClientsideClientState.None;
            Camera = new Camera(World);
            World.WorldChange += World_WorldChange;
        }

        private void World_WorldChange(Location chunkLoc, Location tileLoc, Tile tile, bool fromServer) {
            // send change to server
            Location global = Location.ToWorld(chunkLoc, tileLoc);
            ByteList data = new ByteList();
            data.Append(global.X);
            data.Append(global.Y);
            data.Append(global.Z);
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
