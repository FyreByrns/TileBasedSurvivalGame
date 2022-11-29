//#define EXCESSIVE_DEBUG_PRINTS

using System;
using System.Collections.Generic;
using System.Linq;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {

    namespace Messages {
        [EitherToEither]
        [Intent("msg")]
        class TextMessage : NetMessage {
            public string Text { get; private set; }
            public bool FromServer { get; private set; }
            public int OriginatingID { get; private set; }

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                Text = BodyData.Get<string>(ref readIndex);

                // if there's more data, it's probably from the server
                if (readIndex != BodyData.Length) {
                    FromServer = BodyData.Get<bool>(ref readIndex);
                    if (FromServer) {
                        OriginatingID = BodyData.Get<int>(ref readIndex);
                    }
                }
            }

            public TextMessage() : base(GetIntentAttr<TextMessage>(), Array.Empty<byte>()) { }
            public TextMessage(string text) : base(GetIntentAttr<TextMessage>(), text.FSData()) { }
            public TextMessage(string text, int originatingID) : this(text) {
                List<byte> moreData = new List<byte>();
                moreData.Append(text);
                moreData.Append(true);
                moreData.Append(originatingID);
                SetupData(moreData.ToArray());
            }
        }
    }
}
