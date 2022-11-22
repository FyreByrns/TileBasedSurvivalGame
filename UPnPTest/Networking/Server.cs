using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedSurvivalGame.Networking {
    class Server {
        Random _rng = new Random();
        public int RandomNumber() {
            return _rng.Next();
        }

        public HashSet<UserData> Users { get; }
        = new HashSet<UserData>();

        public Server() {
        }
    }

    class UserData {
        public string Name { get; set; }
        public int ID { get; set; }
    }
}
