using PixelEngine;
using TileBasedSurvivalGame.Networking;

//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame {
    class Engine : Game {
        // singleton
        public static Engine Instance { get; private set; }

        public Client Client { get; set; }
        public Server Server { get; set; }

        public float TickLength { get; set; } = 1f / 30f;
        private float _tickAccumulator = 0;

        public override void OnUpdate(float elapsed) {
            Logger.Log($"{_tickAccumulator} {TickLength}");

            // input
            bool[] mouseButtons = new bool[(int)Mouse.Any];
            bool[] keys = new bool[(int)Key.Any];
            for (Mouse button = 0; button < Mouse.Any; button++) {
                mouseButtons[(int)button] = GetMouse(button).Down;
            }
            for (Key key = 0; key < Key.Any; key++) {
                keys[(int)key] = GetKey(key).Down;
            }
            InputHandler.UpdateMouse(MouseX, MouseY, (int)MouseScroll);
            InputHandler.Update(mouseButtons, keys);

            // fixed tick rate
            _tickAccumulator += elapsed;
            while (_tickAccumulator > TickLength) {
                Client.Tick(this);
                Server?.Tick(this);

                _tickAccumulator -= TickLength;
            }

            // render chunks
            Client.Camera.Render(this, Client.World, Client.CameraLocation);
        }
        
        public Engine(Client client, Server server) {
            Instance = this;
            DUMB_PARALLEL_DRAW = true;

            Construct(400, 225, 2, 2);
            Client = client;
            Server = server;

            // temporary player spawn
            World.Entity player = new World.Entity(new World.Location(4, 4));
            player.Width = 2;
            player.Height = 2;
            player.Controller = new World.PlayerController(player);
            client.World.Entities.Add(player);

            // temporary bindings here, todo: load bindings from file in InputHandler sctor
            InputHandler.BindInput("move_north", Key.Up);
            InputHandler.BindInput("move_south", Key.Down);
            InputHandler.BindInput("move_west", Key.Left);
            InputHandler.BindInput("move_east", Key.Right);
        }
    }
}
