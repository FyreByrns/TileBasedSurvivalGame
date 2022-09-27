
using TileBasedSurvivalGame.Networking;

//// = documentation
// = per-step working comments


namespace TileBasedSurvivalGame {
    class Engine : PixelEngine.Game {
        public Client Client { get; set; }
        public Server Server { get; set; }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);

            Client.Tick(this);
            Server?.Tick(this);
        }

        public Engine(Client client, Server server) {
            Construct(400, 225, 2, 2);
            Client = client;
            Server = server;
        }
    }
}
