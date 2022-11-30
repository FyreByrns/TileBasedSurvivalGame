//#define EXCESSIVE_DEBUG_PRINTS

using System;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [State(ConnectionState.Setup)]
        [ClientToServer]
        [Intent("rqn")]
        class RequestName : NetMessage {
            public string Name;

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                Name = BodyData.Get<string>(ref readIndex);
            }

            public RequestName() : base(GetIntentAttr<RequestName>(), Array.Empty<byte>()) { }
            public RequestName(string name) : base(GetIntentAttr<RequestName>(), name.FSData()) { }
        }
    }
}
