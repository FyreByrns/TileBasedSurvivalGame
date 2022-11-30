//#define EXCESSIVE_DEBUG_PRINTS

using System;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [State(ConnectionState.Setup)]
        [ClientToServer]
        [Intent("setn")]
        class SetName : NetMessage {
            public string Name;

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                Name = BodyData.Get<string>(ref readIndex);
            }

            public SetName() : base(GetIntentAttr<SetName>(), Array.Empty<byte>()) { }
            public SetName(string name) : base(GetIntentAttr<SetName>(), name.FSData()) { }
        }
    }
}
