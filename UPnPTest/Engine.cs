using PixelEngine;
using TileBasedSurvivalGame.Networking;
using TileBasedSurvivalGame.Scenes;

//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame {

    class Engine : Game {
        public Scene CurrentScene { get; private set; }

        public float TickLength { get; set; } = 1f / 30f;
        private float _tickAccumulator = 0;

        private float _fpsPollAccumulator = 0;
        private int _framesSinceLastPoll = 0;
        private int _fps;

        public override void OnCreate() {
            // default scene
            CurrentScene = new AbstractWorldGenVisualizer();
        }

        public override void OnUpdate(float elapsed) {
            UpdateInput();
            Update(elapsed);
            Tick(elapsed);
            Render();

            // fps tracking
            _framesSinceLastPoll++;
            _fpsPollAccumulator += elapsed;
            if(_fpsPollAccumulator >= 1) {
                _fps = _framesSinceLastPoll;
                _framesSinceLastPoll = 0;
                _fpsPollAccumulator = 0;
            }

            CurrentScene = CurrentScene?.Next;
            if(CurrentScene == null) {
                Finish();
            }
            AppName = $"~tbsg {_fps}fps {CurrentScene?.Name}";
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
        void Update(float elapsed) {
            CurrentScene?.Update(this, elapsed);
        }
        void Tick(float elapsed) {
            // fixed tick rate
            _tickAccumulator += elapsed;
            while (_tickAccumulator > TickLength) {
                CurrentScene?.Tick(this);
                _tickAccumulator -= TickLength;
            }
        }
        void Render() {
            CurrentScene?.Render(this);
        }

        public Engine() {
            // load config
            Config.Load();
            // save config, this will create a default config if none was found
            Config.Save();

            Logger.ShowLogs = Config.Log;

            DUMB_PARALLEL_DRAW = true;
            Construct(Config.ScreenWidth, Config.ScreenHeight, Config.PixelSize, Config.PixelSize);

            // temporary bindings here, todo: load bindings from file in InputHandler sctor
            InputHandler.BindInput("move_north", Key.Up);
            InputHandler.BindInput("move_south", Key.Down);
            InputHandler.BindInput("move_west", Key.Left);
            InputHandler.BindInput("move_east", Key.Right);
        }
    }
}
