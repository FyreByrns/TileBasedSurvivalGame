//#define EXCESSIVE_DEBUG_PRINTS

using System;
using System.Collections.Generic;
using System.Linq;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [ServerToClient]
        [Intent("plist")]
        class PlayerList : NetMessage {
            public int[] IDs { get; private set; }

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                IDs = BodyData.Get<int[]>(ref readIndex);
            }

            public PlayerList() : base(GetIntentAttr<PlayerList>(), Array.Empty<byte>()) { }
            public PlayerList(params int[] ids) : this() {
                List<byte> data = new List<byte>();
                data.Append(ids);
                SetupData(data.ToArray());
            }
        }
    }
}
