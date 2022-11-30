//#define EXCESSIVE_DEBUG_PRINTS

using System;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [State(ConnectionState.Setup)]
        [ClientToServer]
        [Intent("rc")]
        class RequestConnection : NetMessage {
            public RequestConnection() : base(GetIntentAttr<RequestConnection>(), Array.Empty<byte>()) { }
        }
    }
}
