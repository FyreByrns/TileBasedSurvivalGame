﻿using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

using TileBasedSurvivalGame.Networking;

//// = documentation
// = per-step working comments

using static TileBasedSurvivalGame.Helper;

namespace TileBasedSurvivalGame {
    internal class Program {
        static void Main(string[] args) {
            //// one user will also be running a server in the background
            // ask in console if you are a session host
            bool hosting = Helper.Ask("Are you the session host?", false);
            if (hosting) {
                Server s = new Server();
            }

        // then, create the client and run it
        // ask what IP and port to connect to
        goto_connectionInit:
            Console.Write("Enter server address.\n> ");

            IPAddress serverAddress = null;
            string ipInput = Console.ReadLine();

            // if no input, assume localhost
            if (ipInput == string.Empty) {
                ipInput = "127.0.0.1";
            }
            // parse input
            if (!IPAddress.TryParse(ipInput, out serverAddress)) {
                goto goto_connectionInit;
            }

            int port = Ask("Enter server port.", 18888);

            Client client = new Client();
            // todo: load values from config files
            client.Construct(400, 225, 2, 2);

            // setup network handler
            NetHandler.Setup(serverAddress, port, true, hosting);
            NetHandler.OnClientMessage(default(NetMessage)); // kickstart clientside state update
            client.Start();
        }
    }
}
