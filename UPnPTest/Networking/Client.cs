using PixelEngine;
using System;
using FSerialization;

//// = documentation
// = per-step working comments

using static TileBasedSurvivalGame.Networking.Client.ClientsideClientState;
using static TileBasedSurvivalGame.Networking.NetMessage.Intent;
using ByteList = System.Collections.Generic.List<byte>;
using PlayerList = System.Collections.Generic.List<TileBasedSurvivalGame.Networking.Player>;

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
        PlayerList Players;

        Player GetPlayerByID(int id) {
            foreach (Player player in Players) {
                if (player.ID == id) {
                    return player;
                }
            }
            return null;
        }

        public void UpdateState() {
            switch (CurrentState) {
                case ClientsideClientState.None: {
                        if (MostRecentMessage == null) {
                            // no message from the server has been sent, so request a connection
                            NetHandler.SendToServer(NetMessage.ConstructToSend(RequestConnection));
                        }
                        else {
                            MostRecentMessage?.RawData.ResetReadIndex();
                            // server might have responded to the connection request
                            if (MostRecentMessage?.MessageIntent == AllowConnection) {
                                // get my id from message
                                MyID = MostRecentMessage.RawData.Get<int>();

                                // request a name
                                // .. get name TODO actually use proper menu
                                Console.Write("Enter your name: ");
                                string name = Console.ReadLine();
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
                            int numberOfPlayers = MostRecentMessage.RawData.Get<int>();
                            Players = new PlayerList(numberOfPlayers);

                            // add players by IDs
                            for (int i = 0; i < numberOfPlayers; i++) {
                                int id = MostRecentMessage.RawData.Get<int>();
                                Players.Add(new Player(id, id == MyID));
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
                        }
                        if (MostRecentMessage?.MessageIntent == SendName) {
                            int id = MostRecentMessage.RawData.Get<int>();
                            Player player = GetPlayerByID(id);
                            player.Name = MostRecentMessage.RawData.Get<string>();

                            foreach (Player p in Players) {
                                Console.WriteLine($"players:\n\t{p.ID}:{p.Remote}:{p.Name}");
                            }
                        }
                        break;
                    }
            }
        }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);

            Draw(MouseX, MouseY, Pixel.Presets.Lime);

            if (GetMouse(Mouse.Left).Pressed) {
                NetHandler.SendToServer(NetMessage.ConstructToSend(Ping));
            }

            // handle state, state changes
            UpdateState();
        }

        public Client() {
            NetHandler.ClientMessage += NetHandler_ClientMessage;
            CurrentState = ClientsideClientState.None;
        }

        private void NetHandler_ClientMessage(NetMessage message) {
            // record most recent message, probably spawns race condition and will bite me later
            MostRecentMessage = message;
            // handle state, state changes
            UpdateState();
        }
    }
}
