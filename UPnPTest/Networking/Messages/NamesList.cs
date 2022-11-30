//#define EXCESSIVE_DEBUG_PRINTS

using System;
using System.Collections.Generic;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [State(ConnectionState.Setup)]
        [ServerToClient]
        [Intent("aln")]
        class NamesList : NetMessage {
            public int[] ClientIDs;
            public string[] Names;

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                ClientIDs = BodyData.Get<int[]>(ref readIndex);
                Names = BodyData.Get<string[]>(ref readIndex);
            }

            public NamesList() : base(GetIntentAttr<NamesList>(), Array.Empty<byte>()) { }
            public NamesList(int[] ids, string[] names) : this() {
                List<byte> data = new List<byte>();
                data.Append(ids);
                data.Append(names);
                SetupData(data.ToArray());
            }
        }
    }
}
