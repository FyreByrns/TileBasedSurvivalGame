using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.Networking {
    class ClientToServer : Attribute { }
    class ServerToClient : Attribute { }
    class EitherToEither : Attribute { }

    class Intent : Attribute {
        public string IntentString { get; }

        public Intent(string intent) {
            IntentString = intent;
        }
    }
}
