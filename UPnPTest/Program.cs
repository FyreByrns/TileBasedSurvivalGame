using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

using TileBasedSurvivalGame.Networking;

//// = documentation
// = per-step working comments

using static TileBasedSurvivalGame.Helper;
using System.Collections.Concurrent;

namespace TileBasedSurvivalGame {
    internal class Program {
        static void Main(string[] args) {
            Engine engine = new Engine();
            engine.Start();
        }
    }
}
