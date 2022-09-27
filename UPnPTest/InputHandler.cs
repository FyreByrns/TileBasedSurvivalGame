using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelEngine;

namespace TileBasedSurvivalGame {
    static class InputHandler {
        public delegate void InputEventHandler(string input, int ticksHeld);
        public static event InputEventHandler Input;

        public static void OnMouse(Mouse mouse, Input input) {

        }
    }
}
