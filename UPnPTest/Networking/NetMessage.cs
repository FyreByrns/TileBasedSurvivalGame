using System;
using System.Collections.Generic;
using System.Net;
using FSerialization;

//// = documentation
// = per-step working comments


namespace TileBasedSurvivalGame.Networking {
    delegate void NetMessageEventHandler(NetMessage message);

    // wrapper around a raw set of bytes sent over the network
    //   layout:
    //     long   : unix timestamp sent : for determining latency
    //     int    : intent              : the type of message
    //     int    : message length      : number of bytes in data
    //     -      :                     :
    //     byte[] : data                : actual data
    partial class NetMessage {
        // header
        public IPEndPoint Sender { get; private set; }
        public DateTime Sent { get; private set; }
        public DateTime Received { get; private set; }
        public TimeSpan Latency => Received - Sent;
        public Intent MessageIntent { get; private set; }

        // body
        public byte[] RawData { get; private set; }

        // other
        public bool Blocked { get; private set; }

        #region operators

        public override bool Equals(object obj) {
            if (obj is null) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (GetType() != obj.GetType()) {
                return false;
            }

            // should never fail considering types are previously determined to match
            NetMessage other = (NetMessage)obj;
            return other.Sender == Sender
                && other.Sent == Sent
                && other.Received == Received
                && other.MessageIntent == MessageIntent
                // not currently comparing array fields for performance reasons
                //&& other.RawData == RawData
                //&& other.HeaderData == HeaderData
                //&& other.BodyData == BodyData
                && other.Blocked == Blocked;
        }
        public static bool operator ==(NetMessage lhs, NetMessage rhs) {
            // hacky truthy-null
            return lhs?.Equals(rhs) ?? (rhs?.Equals(lhs) ?? true);
        }
        public static bool operator !=(NetMessage lhs, NetMessage rhs) {
            return !(lhs == rhs);
        }

        #endregion operators

        #region static functionality

        //// set the sender of a message (only for use with direct-piping)
        public static void SetSender(ref NetMessage message, IPEndPoint newSender) {
            message.Sender = newSender;
        }

        //// construct a message with just intent
        static byte[] _emptyByteArray = new byte[] { };
        public static NetMessage ConstructToSend(Intent intent) {
            return ConstructToSend(intent, _emptyByteArray);
        }
        //// construct a message with both intent and contents
        public static NetMessage ConstructToSend(Intent intent, byte[] data) {
            NetMessage result = new NetMessage();
            result.MessageIntent = intent;
            // to send, only the raw data needed, no parsing of data
            // .. or sender info
            List<byte> rawDataBuf = new List<byte>();
            rawDataBuf.Append(DateTime.Now.Ticks);
            rawDataBuf.Append((int)intent);
            rawDataBuf.Append(data);
            result.RawData = rawDataBuf.ToArray();
            return result;
        }
        //// construct data from raw sent byte[]
        public static NetMessage ConstructFromSent(IPEndPoint sender, byte[] data) {
            // discard blocked messages without parsing
            if (BlockedEndpoints.Contains(sender)) {
                return BlockedMessage;
            }

            NetMessage result = new NetMessage();
            // trivial, no parsing needed
            result.Sender = sender;
            result.Received = DateTime.Now;
            result.RawData = data;

            // parse
            result.Sent = new DateTime(data.Get<long>());
            result.MessageIntent = (Intent)data.Get<int>();
            result.RawData = data.Get<byte[]>();

            return result;
        }

        #endregion static functionality

        #region blocking
        public static NetMessage BlockedMessage =>
            new NetMessage() { Blocked = true };

        public static List<IPEndPoint> BlockedEndpoints { get; } = new List<IPEndPoint>();
        public static void BlockEndpoint(IPEndPoint toBlock) {
            if (!BlockedEndpoints.Contains(toBlock)) {
                BlockedEndpoints.Add(toBlock);
            }
        }
        public static void UnblockEndpoint(IPEndPoint toUnblock) {
            if (BlockedEndpoints.Contains(toUnblock)) {
                BlockedEndpoints.Remove(toUnblock);
            }
        }

        #endregion blocking
    }
}
