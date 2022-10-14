//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame.Scenes {
    abstract class Scene {
        public abstract string Name { get; }
        public abstract Scene Next { get; protected set; }
        public abstract void Update(Engine instance, float elapsed);
        public abstract void Tick(Engine instance);
        public abstract void Render(Engine instance);
    }
}
