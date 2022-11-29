using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.Networking {
    public enum ConnectionState {
        None = 0,

        Setup,
        Connected,
    }

    class ClientToServer : Attribute { }
    class ServerToClient : Attribute { }
    class EitherToEither : Attribute { }

    class State : Attribute {
        public ConnectionState ConnectionState { get; }

        public State(ConnectionState connectionState) {
            ConnectionState = connectionState;
        }
    }

    class Intent : Attribute {
        public string IntentString { get; }

        public Intent(string intent) {
            IntentString = intent;
        }
    }
}
