using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PixelEngine;

using TileBasedSurvivalGame.World;

namespace TileBasedSurvivalGame.Rendering {
    class Camera {
        public int RenderingHeight { get; set; } = 10;

        // caching things
        Location _lastLocation;
        bool _changesSinceLastFrame;
        public void InvalidateCache() {
            _changesSinceLastFrame = true;
        }
    }
}
