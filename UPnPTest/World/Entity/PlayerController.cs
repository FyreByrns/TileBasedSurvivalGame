
using InputMap = System.Collections.Generic.Dictionary<string, System.Action>;

namespace TileBasedSurvivalGame.World {
    class PlayerController : EntityController {
        public override void Update(TiledWorld world) {
            base.Update(world);
        }

        public PlayerController(Entity owner) : base(owner) {
            DesiredLocation = owner.WorldLocation;
            InputHandler.Input += InputReceived;
        }

        private void InputReceived(string input, int ticksHeld) {
            int desiredMovementX, desiredMovementY;
            desiredMovementX = desiredMovementY = 0;

            // instead of a switch statement, use a simple map
            InputMap inputMethods
            = new InputMap() {
                { "move_north"  , () => { desiredMovementY--; } },
                { "move_south", () => { desiredMovementY++; } },
                { "move_west", () => { desiredMovementX--; } },
                { "move_east", () => { desiredMovementX++; } },
            };

            if (inputMethods.TryGetValue(input, out System.Action result)) {
                // if the input is in the input map, resolve it
                result.Invoke();

                // inform that a movement is desired
                DesiredLocation = new Location(desiredMovementX, desiredMovementY) + Owner.WorldLocation;
            }
        }
    }
}
