//#define EXCESSIVE_DEBUG_PRINTS

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FSerialization;

namespace TileBasedSurvivalGame.Networking {
    class NetMessage {
        public IPEndPoint Sender { get; protected set; }
        public int HeaderLength { get; protected set; }
        public byte[] RawData { get; protected set; }
        public int BodyLength { get; protected set; }
        public byte[] BodyData { get; protected set; }

        public string Intent { get; protected set; }
        public DateTime Sent { get; protected set; }

        public byte[] GetHeader() {
            List<byte> header = new List<byte>();
            header.Append(Sent.Ticks);
            header.Append(Intent);
            header.Append(BodyLength);
            return header.ToArray();
        }

        protected virtual void ReadDataToFields() { }

        protected void SetupData(byte[] data) {
            BodyLength = data.Length;

            byte[] header = GetHeader();
            HeaderLength = header.Length;

            RawData = new byte[HeaderLength + BodyLength];
            Array.Copy(header, RawData, HeaderLength);
            Array.Copy(data, 0, RawData, HeaderLength, BodyLength);
        }

        /// <summary>
        /// Construct a NetMessage to send
        /// </summary>
        protected NetMessage(string intent, byte[] data) {
            Intent = intent;
            Sent = DateTime.Now;

            SetupData(data);
        }
        /// <summary>
        /// Construct a NetMessage from network data
        /// </summary>
        public NetMessage(IPEndPoint sender, byte[] data) {
            Sender = sender;
            RawData = data;

            int readIndex = 0;
            Sent = new DateTime(data.Get<long>(ref readIndex));
            Intent = data.Get<string>(ref readIndex);
            BodyLength = data.Get<int>(ref readIndex);
            HeaderLength = readIndex; // all the header data has been read now, so the read index will be the header length

            BodyData = new byte[BodyLength];
            Array.Copy(RawData, HeaderLength, BodyData, 0, BodyLength);
        }

        //// reflection stuff to generate message type data etc
        static Dictionary<string, Type> IntentToType = new Dictionary<string, Type>();

        public static NetMessage MessageToSubtype(NetMessage rawMessage) {
            NetMessage result = null;
            if (IntentToType.ContainsKey(rawMessage.Intent)) {
                result = (NetMessage)Activator.CreateInstance(IntentToType[rawMessage.Intent]);
                result.Sender = rawMessage.Sender;
                result.HeaderLength = rawMessage.HeaderLength;
                result.RawData = rawMessage.RawData;
                result.BodyLength = rawMessage.BodyLength;
                result.BodyData = rawMessage.BodyData;
                result.Intent = rawMessage.Intent;
                result.Sender = rawMessage.Sender;
                result.ReadDataToFields();
            }
            return result;
        }

        protected static string GetIntentAttr<T>() where T : NetMessage {
            Type t = typeof(T);
            return GetIntentAttr(t);
        }
        protected static string GetIntentAttr(Type t) {
            foreach (Attribute attribute in t.GetCustomAttributes()) {
                if (attribute is Intent intent) {
                    return intent.IntentString;
                }
            }
            return "[none]";
        }

        static NetMessage() {
            IEnumerable<Type> messageTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.FullName.Contains("Messages"));
            foreach (Type messageType in messageTypes) {
                string intent = GetIntentAttr(messageType);
                IntentToType[intent] = messageType;
            }
        }
    }

    namespace Messages {
        [ClientToServer]
        [Intent("rc")]
        class RequestConnection : NetMessage {
            public RequestConnection() : base(GetIntentAttr<RequestConnection>(), Array.Empty<byte>()) { }
        }

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
