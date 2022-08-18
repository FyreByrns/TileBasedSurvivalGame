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

namespace TileBasedSurvivalGame {
    internal class Program {
        static void Main(string[] args) {
            //// one user will also be running a server in the background
            // ask in console if you are a session host
            bool hosting = Helper.Ask("Are you the session host?", false);

            // if hosting, start the server
            if (hosting) {
                // ask for a port
                int hostingPort = Ask("What port would you like to use?", 18888);


                // create the server and run in a thread once all init is done
                AddToWaitingActions(() => {
                    Server s = null;
                    Task.Run(() => {
                        try {
                            s = new Server(hostingPort);

                            // todo: load values from config files
                            s.Start();
                        }
                        catch (Exception exception) {
                            Console.WriteLine($"error in server task: {exception}");
                            Console.WriteLine("server closed");
                        }
                    });
                });
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

            Client client = new Client(serverAddress, port);
            // todo: load values from config files
            client.Construct(400, 225, 2, 2);
            AddToWaitingActions(() => {
                client.Start();
            });

            // post-init, run the client and the possible server
            DoAllWaitingActions();
        }
    }
}
