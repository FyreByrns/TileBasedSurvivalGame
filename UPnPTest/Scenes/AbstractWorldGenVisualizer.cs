//// = documentation
// = per-step working comments

using TileBasedSurvivalGame.World.Abstract;

namespace TileBasedSurvivalGame.Scenes {
    class AbstractWorldGenVisualizer : Scene {
        public override string Name => "Abstract World Gen Visualizer";
        public override Scene Next { get; protected set; }

        public AbstractWorld AbstractWorld { get; set; }

        Vector2 _cameraLocation = (0, 0);
        float _cameraZoom = 1;
        Vector2 ScreenToWorld(Vector2 screenLocation) {
            return (_cameraLocation + screenLocation) * _cameraZoom;
        }
        Vector2 WorldToScreen(Vector2 worldLocation) {
            return (worldLocation - _cameraLocation) * _cameraZoom;
        }

        public override void Update(Engine instance, float elapsed) {
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
            if (input == "cam_up") _cameraLocation.y--;
            if (input == "cam_down") _cameraLocation.y++;
            if (input == "cam_left") _cameraLocation.x--;
            if (input == "cam_right") _cameraLocation.x++;

            //_cameraZoom -= 0.99f * InputHandler.MouseScroll;
        }
    }
}
