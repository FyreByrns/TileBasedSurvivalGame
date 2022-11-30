//#define EXCESSIVE_DEBUG_PRINTS

using System;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [State(ConnectionState.Setup)]
        [ServerToClient]
        [Intent("disallown")]
        class DisallowName : NetMessage {
            public DisallowName() : base(GetIntentAttr<DisallowName>(), Array.Empty<byte>()) { }
        }
    }
}
