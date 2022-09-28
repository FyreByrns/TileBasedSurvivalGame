using System.Collections.Generic;
using PixelEngine;
using tri = TileBasedSurvivalGame.Rendering.TileRenderingHandler.TileRenderingInfo;

namespace TileBasedSurvivalGame.Rendering {
    public static class TileRenderingHandler {
        public const int TileSize = 10;

        //// for convenience
        static Pixel c(byte r, byte g, byte b) => new Pixel(r, g, b);
        
        public static Dictionary<string, tri> RenderingInfo { get; }
        = new Dictionary<string, tri>() {
            {"test"            , (c(100, 100, 100), c(50, 50, 50))},
        };

        public static tri GetRenderingInfo(string type) {
            if(RenderingInfo.ContainsKey(type)) return RenderingInfo[type];
            return null;
        }

        public class TileRenderingInfo { 
            public Pixel OutsideColour { get; }
            public Pixel InsideColour { get; }

            public TileRenderingInfo(Pixel outsideColour, Pixel insideColour) {
                OutsideColour = outsideColour;
                InsideColour = insideColour;
            }
            public static implicit operator TileRenderingInfo((Pixel outsideColour, Pixel insideColour) tuple) {
                return new TileRenderingInfo(tuple.outsideColour, tuple.insideColour);
            }
        }
    }
}
