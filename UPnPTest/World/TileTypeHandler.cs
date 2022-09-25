using System.Collections.Generic;

using tti = TileBasedSurvivalGame.World.TileTypeHandler.TileTypeInfo;

namespace TileBasedSurvivalGame.World {
    public static class TileTypeHandler {
        public static Dictionary<string, tti> TypeInfo { get; }
        = new Dictionary<string, tti>() {
            {"air"          , (false, false ) },
            {"test"         , (true , true  ) },
        };

        public static bool Exists(string type) => TypeInfo.ContainsKey(type);
        public static bool Visible(string type) => Exists(type) && TypeInfo[type].Visible;
        public static bool Invisible(string type) => !Visible(type);

        //// holds information about a tiletype
        public class TileTypeInfo {
            public bool Solid { get; }
            public bool Visible { get; }

            public TileTypeInfo(bool solid, bool visible) {
                Solid = solid;
                Visible = visible;
            }

            public static implicit operator TileTypeInfo((bool solid, bool visible) tuple) {
                return new TileTypeInfo(tuple.solid, tuple.visible);
            }
        }
    }
}
