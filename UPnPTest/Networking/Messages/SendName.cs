//#define EXCESSIVE_DEBUG_PRINTS

using System;
using System.Collections.Generic;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [State(ConnectionState.Setup)]
        [ServerToClient]
        [Intent("aln")]
        class SendName : NetMessage {
            public int ClientID;
            public string Name;

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                ClientID = BodyData.Get<int>(ref readIndex);
                Name = BodyData.Get<string>(ref readIndex);
            }

            public SendName() : base(GetIntentAttr<SendName>(), Array.Empty<byte>()) { }
            public SendName(int id, string name) : this() {
                List<byte> data = new List<byte>();
                data.Append(id);
                data.Append(name);
                SetupData(data.ToArray());
            }
        }
    }
}
