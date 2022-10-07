namespace TileBasedSurvivalGame.World {
    class EntityController {
        public Entity Owner { get; protected set; }
        //// for use in the movement step of ticking the world
        public int TicksBetweenMovement { get; set; }
        public int TickAccumulator { get; set; }
        public Location DesiredLocation { get; protected set; }
        public virtual void Update(TiledWorld world) { }

        protected EntityController(Entity owner) {
            Owner = owner;
        }
    }
}
