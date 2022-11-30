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
        static HashSet<Type> ServerToClientMessages = new HashSet<Type>();
        static HashSet<Type> ClientToServerMessages = new HashSet<Type>();
        static Dictionary<ConnectionState, HashSet<Type>> StateLockedMessages = new Dictionary<ConnectionState, HashSet<Type>>();

        public static bool ExpectedOnClient<T>()
            where T : NetMessage {
            return ExpectedOnClient(typeof(T));
        }
        public static bool ExpectedOnClient(Type t) {
            return ServerToClientMessages.Contains(t);
        }
        public static bool ExpectedOnServer<T>()
            where T : NetMessage {
            return ExpectedOnServer(typeof(T));
        }
        public static bool ExpectedOnServer(Type t) {
            return ClientToServerMessages.Contains(t);
        }
        public static bool ExpectedInState<T>(ConnectionState state)
            where T : NetMessage {
            return ExpectedInState(state, typeof(T));
        }
        public static bool ExpectedInState(ConnectionState state, Type t) {
            return StateLockedMessages.ContainsKey(state) && StateLockedMessages[state].Contains(t);
        }

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
        protected static bool ServerToClient(Type t) {
            foreach (Attribute attribute in t.GetCustomAttributes()) {
                if (attribute is ServerToClient || attribute is EitherToEither) {
                    return true;
                }
            }
            return false;
        }
        protected static bool ClientToServer(Type t) {
            foreach (Attribute attribute in t.GetCustomAttributes()) {
                if (attribute is ClientToServer || attribute is EitherToEither) {
                    return true;
                }
            }
            return false;
        }

        protected static bool StateLocked(Type t) {
            foreach (Attribute attribute in t.GetCustomAttributes()) {
                if (attribute is State) {
                    return true;
                }
            }
            return false;
        }
        protected static ConnectionState GetStateLock(Type t) {
            foreach (Attribute attribute in t.GetCustomAttributes()) {
                if (attribute is State state) {
                    return state.ConnectionState;
                }
            }
            return default;
        }

        static NetMessage() {
            IEnumerable<Type> messageTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.FullName.Contains("Messages"));
            foreach (Type messageType in messageTypes) {
                string intent = GetIntentAttr(messageType);
                IntentToType[intent] = messageType;

                if (ServerToClient(messageType)) {
                    ServerToClientMessages.Add(messageType);
                }
                if (ClientToServer(messageType)) {
                    ClientToServerMessages.Add(messageType);
                }
                if (StateLocked(messageType)) {
                    ConnectionState state = GetStateLock(messageType);
                    if (!StateLockedMessages.ContainsKey(state)) {
                        StateLockedMessages[state] = new HashSet<Type>();
                    }
                    StateLockedMessages[state].Add(messageType);
                }
            }
        }
    }
}
