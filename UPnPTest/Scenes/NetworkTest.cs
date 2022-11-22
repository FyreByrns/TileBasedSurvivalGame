using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedSurvivalGame.Networking;

namespace TileBasedSurvivalGame.Scenes {
    internal class NetworkTest : Scene {
        public override string Name => "Network Test [local server]";

        public override Scene Next { get => this; protected set { } }

        public override void Begin(Engine instance) {
            Logger.ShowLogs = true;
            NetHandler.Setup(System.Net.IPAddress.Parse("127.0.0.1"), 12000, true, true);
        }

        public override void Render(Engine instance) {
        }

        public override void Tick(Engine instance) {
        }

        public override void Update(Engine instance, float elapsed) {
        }
    }
}
