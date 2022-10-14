//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame.Scenes {
    class AbstractWorldGenVisualizer : Scene {
        public override string Name => "Abstract World Gen Visualizer";
        public override Scene Next { get; protected set; }

        public override void Update(Engine instance, float elapsed) {
            Next = this;
        }
        public override void Tick(Engine instance) { }
        override public void Render(Engine instance) {

        }
    }
}
