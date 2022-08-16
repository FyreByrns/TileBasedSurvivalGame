//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame.Networking {
    partial class NetMessage {
        public enum Intent : int {
            None = 0,

            //// general utility intentions
            Ping = 1,
            SendString,
            SendNumber,

            //// construction of connections
            //// .. and maintaining lobby state
            RequestConnection, // c request starting a connection
            AllowConnection, //   s allow connection
            RequestDesiredName, //s request the desired username from a client
            DenyDesiredName, //   s deny desired username
            RequestLobbyInfo, //  c request lobby information
            SendPlayerCount, //   s send number of players in the lobby
            SendPlayerIDs, //     s send player IDs 
            RequestPlayerInfo, // c request info for a specific player ID
            SendPlayerInfo, //    s send player info
        }
    }

/*
connecting to server:
 initiation:
 - client sends RequestConnection
 - if the server responds with AllowConnection, continue
 otherwise:
 - initiation begins again
 or:
 - initiation failed
 - show error message
 - exit
 
 name resolution:
 - server requests the client's desired name with RequestDesiredName
 - client sends desired using RequestDesiredName
 - if the name is accepted (not taken, reserved, etc), 
 - server sends AllowConnection, continue
 otherwise:
 - server sends DenyDesiredName
 - name resolution begins again
 
 lobby:
 - client requests lobby information with RequestLobbyInfo
 - server sends SendPlayerCount with the number of client-visible players as data
 - server sends SendPlayerIDs with
 -   int[] ids

 user info:
 - client requests player information with RequestPlayerInfo with requested ID as data
 - server responds with username using SendText
*/
}
