//#define EXCESSIVE_DEBUG_PRINTS

using System;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [ClientToServer]
        [Intent("rc")]
        class RequestConnection : NetMessage {
            public RequestConnection() : base(GetIntentAttr<RequestConnection>(), Array.Empty<byte>()) { }
        }
    }
}
