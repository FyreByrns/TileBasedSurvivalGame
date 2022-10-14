using PixelEngine;
using TileBasedSurvivalGame.Networking;

//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame {
    abstract class Scene { 
        public abstract string Name { get; }
        public abstract Scene Next { get; protected set; }
        public abstract void Update(Engine instance, float elapsed);
        public abstract void Tick(Engine instance);
    }

    class Engine : Game {
        public Client Client { get; set; }
        public Server Server { get; set; }

        public float TickLength { get; set; } = 1f / 30f;
        private float _tickAccumulator = 0;

        public override void OnUpdate(float elapsed) {
            // fixed tick rate
            _tickAccumulator += elapsed;
            while (_tickAccumulator > TickLength) {
                Client.Tick(this);
                Server?.Tick(this);

                _tickAccumulator -= TickLength;
            }

            // render chunks
            foreach(World.Chunk chunk in Client.World.Chunks.Values) {
                if (chunk.Changed) {
                    chunk.RegenerateGraphics(this);
                }
            }

            Client.Camera.Render(this, Client.World, Client.CameraLocation);
        }
        
        void UpdateInput() {
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
        }

        public Engine(Client client, Server server) {
            DUMB_PARALLEL_DRAW = true;

            Construct(400, 225, 2, 2);
            Client = client;
            Server = server;

            // temporary bindings here, todo: load bindings from file in InputHandler sctor
            InputHandler.BindInput("move_north", Key.Up);
            InputHandler.BindInput("move_south", Key.Down);
            InputHandler.BindInput("move_west", Key.Left);
            InputHandler.BindInput("move_east", Key.Right);
        }
    }
}
