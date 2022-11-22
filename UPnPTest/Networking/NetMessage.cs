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

        /// <summary>
        /// Construct a NetMessage to send
        /// </summary>
        protected NetMessage(string intent, byte[] data) {
            Intent = intent;
            Sent = DateTime.Now;
            BodyLength = data.Length;

            byte[] header = GetHeader();
            HeaderLength = header.Length;
            
            RawData = new byte[HeaderLength + BodyLength];
            Array.Copy(header, RawData, HeaderLength);
            Array.Copy(data, 0, RawData, HeaderLength, BodyLength);
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
            public AllowConnection() : base(GetIntentAttr<AllowConnection>(), Array.Empty<byte>()) { }
        }

        [EitherToEither]
        [Intent("msg")]
        class Message : NetMessage {
            public string Text { get; private set; }

            protected override void ReadDataToFields() {
                base.ReadDataToFields();

                int readIndex = 0;
                Text = BodyData.Get<string>(ref readIndex);
            }

            public Message() : base(GetIntentAttr<Message>(), Array.Empty<byte>()) { }
            public Message(string text) : this() {

            }
        }
    }
}
