using PixelEngine;
using TileBasedSurvivalGame.StateMachines;
using TileBasedSurvivalGame.StateMachines.Clientside;
using TileBasedSurvivalGame.StateMachines.Clientside.ConnectionStates;
using System;

//// = documentation
// = per-step working comments

namespace TileBasedSurvivalGame.Networking {
    class Client : Game {
        ConnectionStateMachine _connectionStateMachine;
        public string StringPopup(string query) {
            // todo: actual popup
            // for now, just console.readline
            System.Console.WriteLine(query);
            return
                System.Console.ReadLine();
        }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);

            Draw(MouseX, MouseY, Pixel.Presets.Lime);

            if (GetMouse(Mouse.Left).Pressed) {
                NetHandler.SendToServer(NetMessage.ConstructToSend(NetMessage.Intent.Ping));
            }
        }

        public Client() {
            NetHandler.ClientMessage += NetHandler_ClientMessage;
            _connectionStateMachine = new ConnectionStateMachine(this);
            _connectionStateMachine.CurrentState = new InitiatingConnection(_connectionStateMachine);
        }

        private void NetHandler_ClientMessage(NetMessage message) {
            string oldState = _connectionStateMachine.CurrentState.GetType().Name;
            Console.WriteLine($"c[client] {_connectionStateMachine.CurrentState.GetType().Name}");
            StateMachineHandler.Update(_connectionStateMachine, message);
            Console.WriteLine($"n[client] {_connectionStateMachine.CurrentState.GetType().Name}");
        }
    }
}
