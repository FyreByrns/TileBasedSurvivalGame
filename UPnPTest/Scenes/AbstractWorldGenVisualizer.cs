//// = documentation
// = per-step working comments

using TileBasedSurvivalGame.World.Abstract;

namespace TileBasedSurvivalGame.Scenes {
    class AbstractWorldGenVisualizer : Scene {
        public override string Name => "Abstract World Gen Visualizer";
        public override Scene Next { get; protected set; }

        public AbstractWorld AbstractWorld { get; set; }

        float _lastElapsed;

        Vector2 _cameraLocation;
        float _cameraZoom = 1;
        float _cameraSpeed = 100;
        Vector2 ScreenToWorld(Vector2 screenLocation) {
            return (_cameraLocation + screenLocation) * _cameraZoom;
        }
        Vector2 WorldToScreen(Vector2 worldLocation) {
            return (worldLocation - _cameraLocation) * _cameraZoom;
        }

        public override void Begin(Engine instance) {
            _cameraLocation = new Vector2(instance.ScreenWidth / 2, instance.ScreenHeight / 2) * -1;
        }

        public override void Update(Engine instance, float elapsed) {
            _lastElapsed = elapsed;
            Next = this;
        }
        public override void Tick(Engine instance) { }
        override public void Render(Engine instance) {
            instance.Clear(PixelEngine.Pixel.Empty);
            instance.Draw(WorldToScreen(AbstractWorld.Origin.Position), PixelEngine.Pixel.Random());
        }

        public AbstractWorldGenVisualizer() {
            AbstractWorld = new AbstractWorld();

            InputHandler.Input += InputHandler_Input;
            InputHandler.BindInput("cam_up", PixelEngine.Key.W);
            InputHandler.BindInput("cam_down", PixelEngine.Key.S);
            InputHandler.BindInput("cam_left", PixelEngine.Key.A);
            InputHandler.BindInput("cam_right", PixelEngine.Key.D);
        }

        private void InputHandler_Input(string input, int ticksHeld) {
            if (input == "cam_up") _cameraLocation.y -= _cameraSpeed * _lastElapsed;
            if (input == "cam_down") _cameraLocation.y += _cameraSpeed * _lastElapsed;
            if (input == "cam_left") _cameraLocation.x -= _cameraSpeed * _lastElapsed;
            if (input == "cam_right") _cameraLocation.x += _cameraSpeed * _lastElapsed;
        }
    }
}
