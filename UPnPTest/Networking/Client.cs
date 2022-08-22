using PixelEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

//// = documentation
// = per-step working comments

using TileBasedSurvivalGame.StateMachines.ClientsideConnectionState;
using TileBasedSurvivalGame.StateMachines.ClientsideConnectionState.States;

namespace TileBasedSurvivalGame.Networking {
    class Client : Game {
        ClientsideConnectionStateMachine _connectionState;

        public string StringPopup(string query) {
            // todo: actual popup
            // for now, just console.readline
            System.Console.WriteLine(query);
            return
                System.Console.ReadLine();
        }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);

            if (_connectionState.CurrentState.GetType() == typeof(InitiatingConnection)) {
                //_connectionState.Update(default(NetMessage), _connectionState);
            }

            Draw(MouseX, MouseY, Pixel.Presets.Lime);

            if (GetMouse(Mouse.Left).Pressed) {
                NetHandler.SendToServer(NetMessage.ConstructToSend(NetMessage.Intent.Ping, new byte[0]));
            }
        }

        public Client() {
            _connectionState = new ClientsideConnectionStateMachine(this);
            _connectionState.CurrentState = new InitiatingConnection();
            _connectionState.Enter();
        }
    }
}
