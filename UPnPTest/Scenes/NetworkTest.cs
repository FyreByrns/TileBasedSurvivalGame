﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedSurvivalGame.Networking;
using TileBasedSurvivalGame.Networking.Messages;

namespace TileBasedSurvivalGame.Scenes {
    internal class NetworkTest : Scene {
        public override string Name => "Network Test [local server]";
        public override Scene Next { get => this; protected set { } }

        public override void Begin(Engine instance) {
            Logger.ShowLogs = true;
            NetHandler.Setup(System.Net.IPAddress.Parse("127.0.0.1"), 12000, true, true);

            NetHandler.ServerMessage += ServerMessageReceived;
            NetHandler.ClientMessage += ClientMessageReceived;

            NetHandler.SendToServer(new RequestConnection());
        }

        private void ClientMessageReceived(NetMessage message) {
            if (message is AllowConnection) {
                Logger.Log("---- server accepted connection!");
                NetHandler.SendToServer(new TextMessage("hi! I am the client."));
            }

            if (message is TextMessage textMessage) {
                Logger.Log($"c rcv {textMessage.Text}");
            }
        }

        private void ServerMessageReceived(NetMessage message) {
            if (message is RequestConnection) {
                NetHandler.SendToClient(message.Sender, new AllowConnection());
                NetHandler.SendToClient(message.Sender, new TextMessage("hello there!"));
            }

            if (message is TextMessage textMessage) {
                Logger.Log($"s rcv {textMessage.Text}");
            }
        }

        public override void Render(Engine instance) {
        }

        public override void Tick(Engine instance) {
        }

        public override void Update(Engine instance, float elapsed) {
        }
    }
}
