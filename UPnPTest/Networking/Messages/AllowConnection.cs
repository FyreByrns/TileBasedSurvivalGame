//#define EXCESSIVE_DEBUG_PRINTS

using System;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [State(ConnectionState.Setup)]
        [ServerToClient]
        [Intent("ac")]
        class AllowConnection : NetMessage {
            public int ClientID;

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                ClientID = BodyData.Get<int>(ref readIndex);
            }

            public AllowConnection() : base(GetIntentAttr<AllowConnection>(), Array.Empty<byte>()) { }
            public AllowConnection(int id) : base(GetIntentAttr<AllowConnection>(), id.FSData()) { }
        }
    }
}
