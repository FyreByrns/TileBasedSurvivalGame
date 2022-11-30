using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedSurvivalGame.Networking;
using TileBasedSurvivalGame.Networking.Messages;

using PixelEngine;

using IPEndPoint = System.Net.IPEndPoint;
using im = TileBasedSurvivalGame.ImmediateModeGui;

namespace TileBasedSurvivalGame.Scenes {
    internal class NetworkTest : Scene {
        public override string Name => "Network Test";
        public override Scene Next { get => this; protected set { } }

        public Lobby Lobby { get; private set; }

        public override void Begin(Engine instance) {
        }

        public override void Render(Engine instance) {
            if (Lobby == null) {
                bool server = im.Button(instance, 10, 10, "host");
                bool client = im.Button(instance, 10, 23, "client only");

                if (server || client) {
                    Lobby = new Lobby(true, server);
                }
                return;
            }

            instance.Clear(Pixel.Presets.Grey);
            if (Lobby.Client) {
                instance.FillCircle(new Point(4, 4), 4, Pixel.Presets.Green);
            }
            if (Lobby.Server) {
                instance.FillCircle(new Point(13, 4), 4, Pixel.Presets.Blue);
            }
            instance.DrawText(new Point(20, 1), Lobby?.RemotePlayers?.Count.ToString() ?? "[nul]", Pixel.Presets.Black);
            instance.DrawText(new Point(1, 10), Lobby?.ClientID.ToString(), Pixel.Presets.Black);
        }

        bool t = true;
        public override void Tick(Engine instance) {
        }

        public override void Update(Engine instance, float elapsed) {
        }
    }
}
