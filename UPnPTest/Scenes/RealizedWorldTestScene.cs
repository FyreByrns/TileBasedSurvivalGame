using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedSurvivalGame.Networking;
using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Scenes {
    internal class RealizedWorldTestScene : Scene {
        //// this test scene uses an integrated server.
        Server server;

        public override string Name => "Realized World Test";

        public override Scene Next { get => this; protected set { } }

        public override void Begin(Engine instance) {
            server = new Server();

            NetHandler.Setup(System.Net.IPAddress.Parse("127.0.0.1"), 12000, true, true);

            // setup worlds
        }

        public override void Render(Engine instance) {
            // for now, just debug draw the client world

            int camX = 0, camY = 0;
        }

        public override void Tick(Engine instance) {
        }

        public override void Update(Engine instance, float elapsed) {
        }
    }
}
